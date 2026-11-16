export async function copyText(text) {
  if (navigator.clipboard?.writeText && window.isSecureContext) {
    try {
      await navigator.clipboard.writeText(text);
      return;
    } catch {
      // The API can exist but reject writes under Permissions-Policy or browser
      // permissions. Still try the legacy path for this explicit user action.
    }
  }

  const failure = "Não foi possível copiar. Selecione o texto e copie manualmente.";
  if (typeof document.execCommand !== "function") {
    throw new Error(failure);
  }

  const active = document.activeElement;
  const inputSelection = (active instanceof HTMLInputElement || active instanceof HTMLTextAreaElement)
    && active.selectionStart !== null
    ? [active.selectionStart, active.selectionEnd, active.selectionDirection] : null;
  const selection = window.getSelection();
  const ranges = selection
    ? Array.from({ length: selection.rangeCount }, (_, i) => selection.getRangeAt(i).cloneRange()) : [];
  const area = document.createElement("textarea");
  area.value = text;
  area.readOnly = true;
  area.style.position = "fixed";
  area.style.opacity = "0";
  document.body.appendChild(area);
  try {
    area.focus({ preventScroll: true });
    area.select();
    if (!document.execCommand("copy")) {
      throw new Error(failure);
    }
  } catch {
    throw new Error(failure);
  } finally {
    area.remove();
    if (selection && ranges.length) {
      selection.removeAllRanges();
      ranges.forEach(range => selection.addRange(range));
    }
    if (active instanceof HTMLElement && active.isConnected) {
      active.focus({ preventScroll: true });
      if (inputSelection) active.setSelectionRange(...inputSelection);
    }
  }
}
