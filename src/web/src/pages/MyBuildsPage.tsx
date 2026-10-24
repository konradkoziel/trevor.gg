import { Link } from 'react-router'
import { buildsPath } from '../features/builds/buildLinks'
import { MyBuilds } from '../features/builds/MyBuilds'

// /ja/buildy (MePage checks the login)
export function MyBuildsPage() {
  return (
    <section>
      <div className="section-head">
        <h2 className="page-title">Moje buildy</h2>
        <Link className="button gold" to={buildsPath}>
          Nowy build
        </Link>
      </div>
      <MyBuilds />
    </section>
  )
}
