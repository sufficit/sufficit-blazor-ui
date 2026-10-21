// Answers the press while the server is still deciding.
//
// IsLoading comes from the parent, so the spinner only lights up after the
// click reached the circuit and the diff came back — long enough on a slow link
// to read as a click that did nothing. This listens in the capture phase, so
// the pending class is on the button before Blazor is told about the click;
// when IsLoading arrives, Blazor rewrites the class and the real spinner takes
// over.
const PENDING = "sui-btn--pending";

// A press that never becomes IsLoading must not leave the button pending.
const RELEASE_MS = 8000;

let registered = false;

export function register() {
  if (registered) return;
  registered = true;

  document.addEventListener("click", (event) => {
    const element = event.target?.closest?.("[data-sui-instant-busy]");
    if (!element || element.disabled ||
        element.getAttribute("aria-disabled") === "true" ||
        element.classList.contains(PENDING)) return;

    element.classList.add(PENDING);
    setTimeout(() => element.classList.remove(PENDING), RELEASE_MS);
  }, true);

  // A submit press the browser's constraint validation stops never leaves the
  // page: the click already marked the button, then the form reports a field
  // as invalid and nothing else happens. "invalid" does not bubble, hence the
  // capture phase; it fires once per offending field, before the browser
  // shows its message.
  document.addEventListener("invalid", (event) => {
    const form = event.target?.form;
    if (!form) return;
    form.querySelectorAll("." + PENDING)
      .forEach((element) => element.classList.remove(PENDING));
    if (form.id) {
      document.querySelectorAll(`[form="${CSS.escape(form.id)}"].${PENDING}`)
        .forEach((element) => element.classList.remove(PENDING));
    }
  }, true);

  // A page restored from the back/forward cache never re-renders.
  addEventListener("pageshow", (event) => {
    if (event.persisted) {
      document.querySelectorAll("." + PENDING)
        .forEach((element) => element.classList.remove(PENDING));
    }
  });
}
