import { describe, expect, it } from 'vitest'
import type { MasteryChampion } from '../../api/players'
import type { ChampionStats } from '../../api/stats'
import { championRows } from './championRows'

const played = (championName: string, games: number): ChampionStats => ({
  championName,
  iconUrl: null,
  games,
  wins: 1,
  winRate: 50,
  kda: 3,
  csPerMinute: 6,
  damagePerMinute: 600,
})

const mastery = (championId: string, name: string, points: number): MasteryChampion => ({
  championId,
  name,
  iconUrl: null,
  level: 7,
  points,
})

describe('champion rows', () => {
  it('puts played champions first and joins their mastery by id or name', () => {
    const rows = championRows(
      [played('Ahri', 2), played('MonkeyKing', 5)],
      [mastery('Garen', 'Garen', 900), mastery('MonkeyKing', 'Wukong', 500), mastery('Ahri', 'Ahri', 100)],
    )

    expect(rows.map((r) => [r.name, r.games, r.masteryPoints])).toEqual([
      ['Wukong', 5, 500],
      ['Ahri', 2, 100],
      ['Garen', 0, 900],
    ])
    expect(rows[2].winRate).toBeNull()
  })

  it('works without mastery, as on your own stats', () => {
    expect(championRows([played('Lux', 1)]).map((r) => [r.id, r.masteryPoints])).toEqual([['Lux', null]])
  })
})
