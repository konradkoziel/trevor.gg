// Grey placeholder rows where data is about to appear, so the page doesn't jump when it does
export function Skeleton({ rows = 3, height = 44 }: { rows?: number; height?: number }) {
  return (
    <div className="skeleton-list" role="status" aria-label="Ładuję…">
      {Array.from({ length: rows }, (_, i) => (
        <div key={i} className="skeleton" style={{ height }} />
      ))}
    </div>
  )
}
