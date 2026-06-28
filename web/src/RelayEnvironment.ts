import {
  Environment,
  Network,
  RecordSource,
  Store,
  type FetchFunction,
} from 'relay-runtime'

// Posts to /graphql on the SAME origin — the YARP gateway routes it to the API,
// so there's no CORS and no API base URL to configure.
const fetchFn: FetchFunction = async (request, variables) => {
  const response = await fetch('/graphql', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ query: request.text, variables }),
  })
  return await response.json()
}

export const RelayEnvironment = new Environment({
  network: Network.create(fetchFn),
  store: new Store(new RecordSource()),
})
