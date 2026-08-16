// Clipboard interop for SUICopyToClipboard.
//
// navigator.clipboard only exists in secure contexts (https / localhost). The
// textarea fallback covers plain-http intranet deployments, which is where the
// original component's silent failures were reported from.
export async function copyText(text) {
  if (navigator.clipboard && window.isSecureContext) {
    await navigator.clipboard.writeText(text);
    return;
  }

  const area = document.createElement("textarea");
  area.value = text;
  // Kept off-screen rather than display:none — hidden elements cannot receive
  // the selection that execCommand copies from.
  area.style.position = "fixed";
  area.style.opacity = "0";
  document.body.appendChild(area);
  area.focus();
  area.select();
  try {
    if (!document.execCommand("copy")) {
      throw new Error("execCommand copy rejected");
    }
  } finally {
    area.remove();
  }
}
