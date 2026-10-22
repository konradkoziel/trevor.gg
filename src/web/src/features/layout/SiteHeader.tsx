import { Link } from 'react-router'
import { AccountMenu } from '../account/AccountMenu'
import { MainNav } from '../account/MainNav'

export function SiteHeader() {
  return (
    <header className="site-header">
      <div className="site-header-inner">
        <Link to="/" className="brand" aria-label="Trevor.gg, strona główna">
          <span className="brand-mark" aria-hidden="true">
            T
          </span>
          <span className="brand-name">
            Trevor<span className="brand-tld">.gg</span>
          </span>
        </Link>
        <MainNav />
        <AccountMenu />
      </div>
    </header>
  )
}
