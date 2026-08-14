const navigationKeys = new Set(["ArrowLeft", "ArrowRight", "Home", "End"]);

export function initialize(root) {
  if (!(root instanceof HTMLElement)) {
    throw new Error("SUITabs requires a valid root element.");
  }

  const preventNativeNavigation = event => {
    if (event.target instanceof HTMLElement
        && event.target.getAttribute("role") === "tab"
        && navigationKeys.has(event.key)) {
      event.preventDefault();
    }
  };

  root.addEventListener("keydown", preventNativeNavigation);

  return {
    dispose() {
      root.removeEventListener("keydown", preventNativeNavigation);
    }
  };
}
