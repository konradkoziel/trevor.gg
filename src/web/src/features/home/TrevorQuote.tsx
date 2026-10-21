import { useState } from 'react'
import { pickDaily } from './daily'
import { trevorQuotes } from './trevorQuotes'

export function TrevorQuote() {
  // Today, fixed for the life of the page so a re-render can't switch the quote
  const [today] = useState(() => new Date())
  const quote = pickDaily(trevorQuotes, today, 'quote')
  return (
    <figure className="quote">
      <blockquote>{quote}</blockquote>
      <figcaption className="muted">Trevor, cytat dnia</figcaption>
    </figure>
  )
}
