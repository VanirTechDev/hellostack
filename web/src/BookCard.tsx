import { graphql, useFragment } from 'react-relay'
import type { BookCard_book$key } from './__generated__/BookCard_book.graphql'

// A Relay fragment colocated with the component that renders it. The Library list
// only knows it needs "...BookCard_book" on each node — this component owns the fields.
const BookFragment = graphql`
  fragment BookCard_book on Book {
    title
    author
    sequenceNumber
    publicationYear
    pages
  }
`

export function BookCard({ book }: { book: BookCard_book$key }) {
  const b = useFragment(BookFragment, book)
  const label = b.sequenceNumber === 0 ? 'Prequel' : `Book ${b.sequenceNumber}`
  return (
    <li className="card">
      <div className="card-top">
        <span className="num">{label}</span>
        <span className="pages">{b.pages} pp</span>
      </div>
      <h2>{b.title}</h2>
      <p className="meta">{b.author} · {b.publicationYear}</p>
    </li>
  )
}
