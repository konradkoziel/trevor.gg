import { useState } from 'react'
import { Link } from 'react-router'
import { fetchBuildData } from '../../api/buildData'
import type { Champion } from '../../api/champions'
import { useApi } from '../../api/useApi'
import {
  addItem,
  maxBuildNameLength,
  moveItem,
  newBuild,
  removeItem,
  rename,
  clearSummonerSpell,
  setSummonerSpell,
  type Build,
} from './build'
import { championBuildsPath } from './buildLinks'
import { ItemsSection } from './ItemsSection'
import { RunesSection } from './RunesSection'
import { SaveBar } from './SaveBar'
import { SkillOrderSection } from './SkillOrderSection'
import { SummonersSection } from './SummonersSection'

interface Props {
  champion: Champion
  // An existing build: editable by its owner, read-only for everyone else
  saved?: { id: string; build: Build; ownerName: string; isMine: boolean }
}

export function BuildEditor({ champion, saved }: Props) {
  const data = useApi(fetchBuildData)
  const [build, setBuild] = useState(() => saved?.build ?? newBuild(champion.id))
  const readOnly = saved !== undefined && !saved.isMine

  return (
    <section className="editor">
      <header className="editor-header">
        <img src={champion.iconUrl} alt="" width={64} height={64} />
        <div className="editor-title">
          <h2>{champion.name}</h2>
          {readOnly ? (
            <p className="build-name">
              {build.name} <span className="muted">· {saved.ownerName}</span>
            </p>
          ) : (
            <input
              className="search"
              placeholder={`Nazwa buildu, np. „${champion.name} na lajnie, który boli”`}
              value={build.name}
              maxLength={maxBuildNameLength}
              onChange={(e) => setBuild((b) => rename(b, e.target.value))}
            />
          )}
        </div>
        <Link className="button secondary" to={championBuildsPath(champion.id)}>
          {readOnly ? 'Inne buildy' : 'Wróć'}
        </Link>
      </header>

      {/* A disabled fieldset turns every button inside off at once: the read-only view */}
      <fieldset className="editor-body" disabled={readOnly}>
        {data.status === 'loading' && <p className="muted">Ładuję itemy i runy…</p>}
        {data.status === 'error' && <p className="error">Riot nie oddał itemów. Odśwież za chwilę.</p>}
        {data.status === 'ready' && (
          <>
            <ItemsSection
              items={data.data.items}
              selected={build.items}
              readOnly={readOnly}
              onAdd={(id) => setBuild((b) => addItem(b, id))}
              onRemove={(index) => setBuild((b) => removeItem(b, index))}
              onMove={(index, offset) => setBuild((b) => moveItem(b, index, offset))}
            />
            <RunesSection
              trees={data.data.runeTrees}
              page={build.runes}
              readOnly={readOnly}
              onChange={(runes) => setBuild((b) => ({ ...b, runes }))}
            />
            <SummonersSection
              spells={data.data.summonerSpells}
              selected={build.summonerSpells}
              readOnly={readOnly}
              onPick={(slot, id) => setBuild((b) => setSummonerSpell(b, slot, id))}
              onClear={(slot) => setBuild((b) => clearSummonerSpell(b, slot))}
            />
          </>
        )}
        <SkillOrderSection
          championId={champion.id}
          order={build.skillOrder}
          onChange={(skillOrder) => setBuild((b) => ({ ...b, skillOrder }))}
        />
      </fieldset>

      {!readOnly && <SaveBar build={build} buildId={saved?.id} />}
    </section>
  )
}
