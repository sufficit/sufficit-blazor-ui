import { readFile, writeFile } from "node:fs/promises";
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
  raw: 52_000,
  gzip: 9_500,
  brotli: 8_000,
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

process.stdout.write(`${checkOnly ? "checked" : "generated"} sufficit-ui.css `
  + `(raw=${sizes.raw}, gzip=${sizes.gzip}, brotli=${sizes.brotli})\n`);
