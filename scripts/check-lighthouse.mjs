#!/usr/bin/env node
// Gates a Lighthouse JSON report against the committed thresholds.
// Usage: node scripts/check-lighthouse.mjs <report.json>
//
// Category scores guard the overall experience; the metric assertions guard the
// numbers a category score can hide, and the budget audit guards payload growth.
// Raising a threshold requires a reason in the pull request, not a quiet edit.

import { readFile } from 'node:fs/promises'

const CATEGORY_THRESHOLDS = {
  // Timing-driven, so it keeps a little headroom for runner variance.
  performance: 0.95,
  // Deterministic audits: the catalog scores a clean 1.0 and must keep it.
  accessibility: 1.0,
  'best-practices': 1.0,
  seo: 1.0,
}

const METRIC_THRESHOLDS = {
  'first-contentful-paint': 1500,
  'largest-contentful-paint': 2500,
  'total-blocking-time': 200,
  'cumulative-layout-shift': 0.1,
  'speed-index': 2000,
}

const reportPath = process.argv[2]
if (!reportPath) {
  console.error('usage: node scripts/check-lighthouse.mjs <report.json>')
  process.exit(2)
}

const report = JSON.parse(await readFile(reportPath, 'utf8'))
const failures = []

for (const [category, threshold] of Object.entries(CATEGORY_THRESHOLDS)) {
  const score = report.categories?.[category]?.score
  if (typeof score !== 'number') {
    failures.push(`category ${category}: missing from the report`)
    continue
  }

  if (score < threshold) {
    failures.push(`category ${category}: ${score.toFixed(2)} < ${threshold}`)
  }
}

const metrics = report.audits?.metrics?.details?.items?.[0] ?? {}
const metricKeys = {
  'first-contentful-paint': 'firstContentfulPaint',
  'largest-contentful-paint': 'largestContentfulPaint',
  'total-blocking-time': 'totalBlockingTime',
  'cumulative-layout-shift': 'cumulativeLayoutShift',
  'speed-index': 'speedIndex',
}

for (const [metric, budget] of Object.entries(METRIC_THRESHOLDS)) {
  const value = metrics[metricKeys[metric]]
  if (typeof value !== 'number') continue

  if (value > budget) {
    failures.push(`metric ${metric}: ${value} > ${budget}`)
  }
}

for (const item of report.audits?.['performance-budget']?.details?.items ?? []) {
  const over = item.sizeOverBudget ?? item.countOverBudget
  if (over) {
    failures.push(`budget ${item.label ?? item.resourceType}: over by ${over}`)
  }
}

const summary = Object.entries(report.categories ?? {})
  .map(([name, category]) => `${name}=${category.score}`)
  .join(' ')

if (failures.length > 0) {
  console.error(`lighthouse gate failed (${summary})`)
  for (const failure of failures) console.error(`  - ${failure}`)
  process.exit(1)
}

console.log(`lighthouse gate passed: ${summary}`)
for (const [metric, key] of Object.entries(metricKeys)) {
  if (typeof metrics[key] === 'number') console.log(`  ${metric}: ${metrics[key]}`)
}
