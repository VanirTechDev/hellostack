import { Suspense, useState, useTransition } from 'react'
import {
  RelayEnvironmentProvider,
  useLazyLoadQuery,
  usePaginationFragment,
  graphql,
} from 'react-relay'
import { RelayEnvironment } from './RelayEnvironment'
import { BookCard } from './BookCard'
import type { AppLibraryQuery as AppLibraryQueryType } from './__generated__/AppLibraryQuery.graphql'
import type { App_books$key } from './__generated__/App_books.graphql'
import './App.css'

const PAGE = 6

const LibraryQuery = graphql`
  query AppLibraryQuery($first: Int!, $order: [BookSortInput!], $where: BookFilterInput) {
    ...App_books @arguments(first: $first, order: $order, where: $where)
  }
`

// A @refetchable + @connection fragment on Query: usePaginationFragment drives both
// "load more" (loadNext) and sort/filter changes (refetch), all from one root query.
const BooksFragment = graphql`
  fragment App_books on Query
    @refetchable(queryName: "LibraryPaginationQuery")
    @argumentDefinitions(
      first: { type: "Int", defaultValue: 6 }
      after: { type: "String" }
      order: { type: "[BookSortInput!]" }
      where: { type: "BookFilterInput" }
    ) {
    books(first: $first, after: $after, order: $order, where: $where)
      @connection(key: "App_books") {
      totalCount
      edges {
        node {
          id
          ...BookCard_book
        }
      }
    }
  }
`

const SORTS = {
  series: [{ sequenceNumber: 'ASC' }],
  newest: [{ publicationYear: 'DESC' }],
  longest: [{ pages: 'DESC' }],
  title: [{ title: 'ASC' }],
} as const
type SortKey = keyof typeof SORTS

function Library({ queryRef }: { queryRef: App_books$key }) {
  const { data, loadNext, hasNext, isLoadingNext, refetch } = usePaginationFragment(
    BooksFragment,
    queryRef,
  )
  const [isPending, startTransition] = useTransition()
  const [sort, setSort] = useState<SortKey>('series')
  const [author, setAuthor] = useState('')
  const [search, setSearch] = useState('')

  function apply(next: { sort?: SortKey; author?: string; search?: string }) {
    const s = next.sort ?? sort
    const a = next.author ?? author
    const q = next.search ?? search
    const where: Record<string, unknown> = {}
    if (a) where.author = { eq: a }
    if (q) where.title = { contains: q }
    startTransition(() => {
      refetch({
        first: PAGE,
        order: SORTS[s] as unknown as never,
        where: (Object.keys(where).length ? where : null) as never,
      })
    })
  }

  const edges = data.books?.edges ?? []
  const total = data.books?.totalCount ?? 0

  return (
    <main className="wrap">
      <header className="head">
        <span className="tag">aspire · hotchocolate · relay</span>
        <h1>The Wheel of Time</h1>
        <p>A Relay-paginated, filterable, sortable library — {total} books in the current result.</p>
      </header>

      <div className={`controls${isPending ? ' pending' : ''}`}>
        <label>
          Sort
          <select
            value={sort}
            onChange={(e) => {
              const v = e.target.value as SortKey
              setSort(v)
              apply({ sort: v })
            }}
          >
            <option value="series">Series order</option>
            <option value="newest">Newest first</option>
            <option value="longest">Longest first</option>
            <option value="title">Title A–Z</option>
          </select>
        </label>
        <label>
          Author
          <select
            value={author}
            onChange={(e) => {
              setAuthor(e.target.value)
              apply({ author: e.target.value })
            }}
          >
            <option value="">All authors</option>
            <option value="Robert Jordan">Robert Jordan</option>
            <option value="Brandon Sanderson">Brandon Sanderson</option>
          </select>
        </label>
        <label className="search">
          Search title
          <input
            value={search}
            placeholder="e.g. Dragon"
            onChange={(e) => {
              setSearch(e.target.value)
              apply({ search: e.target.value })
            }}
          />
        </label>
      </div>

      <ol className="grid">
        {edges.map((e) => e?.node && <BookCard key={e.node.id} book={e.node} />)}
      </ol>

      <div className="more">
        {hasNext ? (
          <button disabled={isLoadingNext} onClick={() => loadNext(PAGE)}>
            {isLoadingNext ? 'Loading…' : `Load more — ${edges.length} of ${total}`}
          </button>
        ) : (
          <span className="done">Showing all {edges.length} of {total}</span>
        )}
      </div>
    </main>
  )
}

function Root() {
  const data = useLazyLoadQuery<AppLibraryQueryType>(LibraryQuery, { first: PAGE })
  return <Library queryRef={data} />
}

export default function App() {
  return (
    <RelayEnvironmentProvider environment={RelayEnvironment}>
      <Suspense fallback={<main className="wrap"><h1 className="loading">Loading…</h1></main>}>
        <Root />
      </Suspense>
    </RelayEnvironmentProvider>
  )
}
