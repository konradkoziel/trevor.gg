import { useState } from 'react'
import { fetchBuildData } from '../../api/buildData'
import type { Champion } from '../../api/champions'
import { useApi } from '../../api/useApi'
import { addItem, maxBuildNameLength, moveItem, newBuild, removeItem, rename, toggleSummonerSpell } from './build'
import { ItemsSection } from './ItemsSection'
import { RunesSection } from './RunesSection'
import { SkillOrderSection } from './SkillOrderSection'
import { SummonersSection } from './SummonersSection'

interface Props {
  champion: Champion
  onChangeChampion: () => void
}

export function BuildEditor({ champion, onChangeChampion }: Props) {
  const data = useApi(fetchBuildData)
  const [build, setBuild] = useState(() => newBuild(champion.id))

  return (
    <section className="editor">
      <header className="editor-header">
        <img src={champion.iconUrl} alt="" width={64} height={64} />
        <div className="editor-title">
          <h2>{champion.name}</h2>
          <input
            className="search"
            placeholder={`Nazwa buildu, np. „${champion.name} na lajnie, który boli”`}
            value={build.name}
            maxLength={maxBuildNameLength}
            onChange={(e) => setBuild((b) => rename(b, e.target.value))}
          />
        </div>
        <button type="button" className="button secondary" onClick={onChangeChampion}>
          Zmień championa
        </button>
      </header>

      {data.status === 'loading' && <p className="muted">Ładuję itemy i runy…</p>}
      {data.status === 'error' && <p className="error">Riot nie oddał itemów. Odśwież za chwilę.</p>}
      {data.status === 'ready' && (
        <>
          <ItemsSection
            items={data.data.items}
            selected={build.items}
            onAdd={(id) => setBuild((b) => addItem(b, id))}
            onRemove={(index) => setBuild((b) => removeItem(b, index))}
            onMove={(index, offset) => setBuild((b) => moveItem(b, index, offset))}
          />
          <RunesSection trees={data.data.runeTrees} page={build.runes} onChange={(runes) => setBuild((b) => ({ ...b, runes }))} />
          <SummonersSection
            spells={data.data.summonerSpells}
            selected={build.summonerSpells}
            onToggle={(id) => setBuild((b) => toggleSummonerSpell(b, id))}
          />
        </>
      )}
      <SkillOrderSection
        championId={champion.id}
        order={build.skillOrder}
        onChange={(skillOrder) => setBuild((b) => ({ ...b, skillOrder }))}
      />

      <footer className="editor-footer">
        <button type="button" className="button discord" disabled title="Zapisywanie buildów w następnej wersji">
          Zapisz build
        </button>
        <span className="muted">Zapisywanie i linki do buildów już w następnej wersji.</span>
      </footer>
    </section>
  )
}
