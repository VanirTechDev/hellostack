import { Suspense } from 'react'
import {
  RelayEnvironmentProvider,
  useLazyLoadQuery,
  graphql,
} from 'react-relay'
import { RelayEnvironment } from './RelayEnvironment'
import type { AppHelloQuery as AppHelloQueryType } from './__generated__/AppHelloQuery.graphql'
import './App.css'

const AppHelloQuery = graphql`
  query AppHelloQuery {
    messages(first: 1, order: [{ createdAt: DESC }]) {
      nodes {
        text
      }
    }
  }
`

function Hello() {
  const data = useLazyLoadQuery<AppHelloQueryType>(AppHelloQuery, {})
  const message = data.messages?.nodes?.[0]?.text ?? '…'
  return (
    <main className="card">
      <span className="tag">aspire · hotchocolate · relay</span>
      <h1>{message}</h1>
      <p>Served from Postgres, through GraphQL, over a single YARP origin.</p>
    </main>
  )
}

export default function App() {
  return (
    <RelayEnvironmentProvider environment={RelayEnvironment}>
      <Suspense fallback={<main className="card"><h1>Loading…</h1></main>}>
        <Hello />
      </Suspense>
    </RelayEnvironmentProvider>
  )
}
