import type { RuneTree } from '../../api/buildData'
import {
  minorRows,
  selectPrimaryRune,
  selectPrimaryTree,
  selectSecondaryRune,
  selectSecondaryTree,
  type RunePage,
} from './runes'
import { Tooltip } from '../tooltip/Tooltip'

interface Props {
  trees: RuneTree[]
  page: RunePage
  // Shared build of someone else: only the chosen trees, no hints
  readOnly?: boolean
  onChange: (page: RunePage) => void
}

export function RunesSection({ trees, page, readOnly = false, onChange }: Props) {
  const primary = trees.find((t) => t.id === page.primaryTreeId)
  const secondary = trees.find((t) => t.id === page.secondaryTreeId)
  const shown = (candidates: RuneTree[], selectedId: number | null) =>
    readOnly ? candidates.filter((t) => t.id === selectedId) : candidates

  if (readOnly && !primary && !secondary)
    return (
      <section className="editor-section">
        <h3>Runy</h3>
        <p className="muted">Bez run. Odważnie.</p>
      </section>
    )

  return (
    <section className="editor-section">
      <h3>Runy</h3>
      <div className="rune-columns">
        <div>
          <TreeTabs
            trees={shown(trees, page.primaryTreeId)}
            selectedId={page.primaryTreeId}
            onSelect={(id) => onChange(selectPrimaryTree(page, id))}
          />
          {primary
            ? primary.slots.map((row, rowIndex) => (
                <RuneRow
                  key={rowIndex}
                  runes={row}
                  keystone={rowIndex === 0}
                  isSelected={(id) => page.primary[rowIndex] === id}
                  onSelect={(id) => onChange(selectPrimaryRune(page, rowIndex, id))}
                />
              ))
            : !readOnly && <p className="muted">Wybierz główną ścieżkę.</p>}
        </div>
        <div>
          <TreeTabs
            trees={shown(
              trees.filter((t) => t.id !== page.primaryTreeId),
              page.secondaryTreeId,
            )}
            selectedId={page.secondaryTreeId}
            onSelect={(id) => onChange(selectSecondaryTree(page, id))}
          />
          {secondary
            ? minorRows.map((rowIndex) => (
                <RuneRow
                  key={rowIndex}
                  runes={secondary.slots[rowIndex] ?? []}
                  isSelected={(id) => page.secondary.some((r) => r.runeId === id)}
                  onSelect={(id) => onChange(selectSecondaryRune(page, rowIndex, id))}
                />
              ))
            : !readOnly && <p className="muted">I poboczną. Dwie runy z różnych rzędów.</p>}
        </div>
      </div>
    </section>
  )
}

function TreeTabs({
  trees,
  selectedId,
  onSelect,
}: {
  trees: RuneTree[]
  selectedId: number | null
  onSelect: (id: number) => void
}) {
  return (
    <div className="tree-tabs">
      {trees.map((tree) => (
        <button
          key={tree.id}
          type="button"
          className={tree.id === selectedId ? 'tree-tab selected' : 'tree-tab'}
          onClick={() => onSelect(tree.id)}
          title={tree.name}
          aria-label={tree.name}
        >
          <img src={tree.iconUrl} alt="" width={28} height={28} />
          {/* Only the chosen tree is named, so five trees fit one row */}
          {tree.id === selectedId && <span>{tree.name}</span>}
        </button>
      ))}
    </div>
  )
}

interface RuneRowProps {
  runes: RuneTree['slots'][number]
  keystone?: boolean
  isSelected: (runeId: number) => boolean
  onSelect: (runeId: number) => void
}

function RuneRow({ runes, keystone = false, isSelected, onSelect }: RuneRowProps) {
  const size = keystone ? 52 : 36
  return (
    <div className="rune-row">
      {runes.map((rune) => (
        <Tooltip key={rune.id} title={rune.name} text={rune.description}>
          <button
            type="button"
            className={isSelected(rune.id) ? 'rune selected' : 'rune'}
            onClick={() => onSelect(rune.id)}
            aria-pressed={isSelected(rune.id)}
          >
            <img src={rune.iconUrl} alt={rune.name} width={size} height={size} />
          </button>
        </Tooltip>
      ))}
    </div>
  )
}
