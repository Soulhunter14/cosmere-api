/**
 * import_sessions.mjs
 *
 * Reads Obsidian session files and outputs sessions.ts for the React app.
 *
 * Usage:
 *   node import_sessions.mjs
 *
 * Source:  C:\Users\xavie\Documents\Obsidian\CosmereRpg\CosmereRpg\Diario\Sesiones\
 * Output:  C:\Users\xavie\Documents\Repositories\personal\cosmere-web\src\data\sessions.ts
 */

import { readdir, readFile, writeFile } from 'fs/promises'
import { join } from 'path'

const VAULT_SESSIONS = 'C:/Users/xavie/Documents/Obsidian/CosmereRpg/CosmereRpg/Diario/Sesiones'
const OUTPUT_FILE    = 'C:/Users/xavie/Documents/Repositories/personal/cosmere-web/src/data/sessions.ts'

// ── Helpers ────────────────────────────────────────────────────────────────

function parseMention(raw) {
  // raw = "PJ - Guizmo" | "NPC - Kaiana" | "Spren - Magma" | "Facción - Sangre Espectral"
  const lower = raw.toLowerCase()
  let type = 'unknown'
  let display = raw

  if (lower.startsWith('pj -')) {
    type = 'pj'
    display = raw.replace(/^pj\s*-\s*/i, '').trim()
  } else if (lower.startsWith('npc -')) {
    type = 'npc'
    display = raw.replace(/^npc\s*-\s*/i, '').trim()
  } else if (lower.startsWith('spren -')) {
    type = 'spren'
    display = raw.replace(/^spren\s*-\s*/i, '').trim()
  } else if (lower.startsWith('facción -') || lower.startsWith('faccion -')) {
    type = 'faction'
    display = raw.replace(/^facci[oó]n\s*-\s*/i, '').trim()
  }

  return { raw, display, type }
}

function extractMentions(body) {
  const mentions = []
  const seen = new Set()
  const regex = /\[\[([^\]]+)\]\]/g
  let match
  while ((match = regex.exec(body)) !== null) {
    const raw = match[1].trim()
    if (!seen.has(raw)) {
      seen.add(raw)
      mentions.push(parseMention(raw))
    }
  }
  return mentions
}

function parseFile(filename, content) {
  // Filename: "Sesión 01 - El Encuentro.md"
  const nameMatch = filename.match(/Sesi[oó]n\s+(\d+)\s+-\s+(.+)\.md$/i)
  const number = nameMatch ? parseInt(nameMatch[1], 10) : 0
  const title  = nameMatch ? nameMatch[2].trim() : filename.replace('.md', '')

  // Strip H1 line and tag lines (#sesion etc.)
  const lines = content.split('\n')
  const body = lines
    .filter(line => !line.startsWith('#') || line.startsWith('## '))
    .join('\n')
    .trim()

  const mentions = extractMentions(body)
  const participants = mentions.filter(m => m.type === 'pj').map(m => m.display)

  // Preview: first non-empty line of body
  const preview = body.split('\n').find(l => l.trim().length > 0)?.trim() ?? ''

  return { number, title, slug: `sesion-${String(number).padStart(2, '0')}`, body, preview, participants, mentions }
}

// ── Main ───────────────────────────────────────────────────────────────────

const files = (await readdir(VAULT_SESSIONS))
  .filter(f => f.endsWith('.md'))
  .sort()

const sessions = []
for (const filename of files) {
  const content = await readFile(join(VAULT_SESSIONS, filename), 'utf8')
  sessions.push(parseFile(filename, content))
}

sessions.sort((a, b) => a.number - b.number)

// ── Output ─────────────────────────────────────────────────────────────────

const ts = `// AUTO-GENERATED — do not edit manually.
// Run: node cosmere-api/Resources/obsidian/import_sessions.mjs

export type MentionType = 'pj' | 'npc' | 'spren' | 'faction' | 'unknown'

export interface SessionMention {
  raw: string
  display: string
  type: MentionType
}

export interface Session {
  number: number
  title: string
  slug: string
  preview: string
  body: string
  participants: string[]
  mentions: SessionMention[]
}

export const SESSIONS: Session[] = ${JSON.stringify(sessions, null, 2)}
`

await writeFile(OUTPUT_FILE, ts, 'utf8')
console.log(`✓ ${sessions.length} sessions written to ${OUTPUT_FILE}`)
