import { readFile, writeFile, readdir, mkdir } from "node:fs/promises";
import { brotliCompressSync, gzipSync, constants as zlibConstants } from "node:zlib";
import { fileURLToPath } from "node:url";
import path from "node:path";
import process from "node:process";
import { bundle } from "lightningcss";

const scriptDirectory = path.dirname(fileURLToPath(import.meta.url));
const repositoryRoot = path.resolve(scriptDirectory, "..");
const entryPath = path.join(repositoryRoot, "src", "styles", "sui-entry.css");
const outputPath = path.join(repositoryRoot, "src", "wwwroot", "sufficit-ui.css");
const checkOnly = process.argv.includes("--check");

const budgets = {
  // Soft buttons add 235 B raw / 41 B gzip after removing redundant rules.
  // Includes viewport-safe autocomplete and semantic action contrasts (2026-09 review).
  // Includes the accessible responsive drawer, focus-safe full-screen mode,
  // safe-area handling, navigation overlay states, and the opt-in labelled
  // mobile-table presentation. The package-level ceiling remains 56 KiB.
  // Button label flex/alignment adds 60 B raw; wrapping uses the existing root policy.
  // Shared pressed feedback and tonal border; compressed budget remains below 10 KiB.
  // Headroom rule: measured × 1.03 rounded up (see AssetBudgetTests). Measured
  // 2026-09-11: raw 54,751 / gzip 10,098 / brotli 8,840 B.
  raw: 56_400,
  gzip: 10_400,
  brotli: 9_110,
};

const result = bundle({
  filename: entryPath,
  minify: true,
  sourceMap: false,
});
const generated = result.code;
const sizes = {
  raw: generated.byteLength,
  gzip: gzipSync(generated, { level: 9 }).byteLength,
  brotli: brotliCompressSync(generated, {
    params: {
      [zlibConstants.BROTLI_PARAM_QUALITY]: 11,
    },
  }).byteLength,
};

for (const [kind, size] of Object.entries(sizes)) {
  if (size > budgets[kind]) {
    throw new Error(`CSS ${kind} budget exceeded: ${size} > ${budgets[kind]} bytes`);
  }
}

if (checkOnly) {
  const committed = await readFile(outputPath);
  if (!committed.equals(generated)) {
    throw new Error("src/wwwroot/sufficit-ui.css is stale; run npm run build:css");
  }
} else {
  await writeFile(outputPath, generated);
}

// Preserve legacy asset URLs from the same authoring tree.
const sourceDirectory = path.join(repositoryRoot, "src", "styles");
const legacyDirectory = path.join(repositoryRoot, "src", "wwwroot", "styles");
await mkdir(legacyDirectory, { recursive: true });
for (const file of await readdir(sourceDirectory)) {
  if (!file.endsWith(".css")) continue;
  const source = await readFile(path.join(sourceDirectory, file));
  const destination = path.join(legacyDirectory, file);
  if (checkOnly) {
    const current = await readFile(destination);
    if (!source.equals(current)) throw new Error(`Legacy CSS is stale: ${file}`);
  } else await writeFile(destination, source);
}

process.stdout.write(`${checkOnly ? "checked" : "generated"} sufficit-ui.css `
  + `(raw=${sizes.raw}, gzip=${sizes.gzip}, brotli=${sizes.brotli})\n`);
