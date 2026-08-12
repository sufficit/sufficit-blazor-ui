(function (root) {
    "use strict";

    const DEFAULTS = Object.freeze({
        containerSelector: "[data-sui-align-row]",
        fieldSelector:
            ":scope > [data-sui-align-field], :scope > .field-group, :scope > .sui-field",
        labelSelector:
            ":scope > label, :scope > legend, :scope > [data-sui-field-label], :scope > span",
        controlSelector:
            ":scope > input, :scope > select, :scope > textarea, :scope > .sui-input, :scope > .sui-select__trigger, :scope > [data-sui-field-control]",
        tolerance: 2
    });

    function isFiniteRect(rect) {
        return rect
            && ["top", "right", "bottom", "left", "width", "height"]
                .every((key) => Number.isFinite(rect[key]));
    }

    function areHorizontalPeers(left, right) {
        if (!isFiniteRect(left) || !isFiniteRect(right)) return false;
        const separatedHorizontally =
            left.right <= right.left || right.right <= left.left;
        const overlap = Math.min(left.bottom, right.bottom)
            - Math.max(left.top, right.top);
        const minimumOverlap = Math.min(left.height, right.height) * 0.25;
        return separatedHorizontally && overlap >= minimumOverlap;
    }

    function comparePeerGeometry(left, right, tolerance) {
        if (!areHorizontalPeers(left.field, right.field)) return [];
        const failures = [];
        for (const dimension of ["field", "label", "control"]) {
            if (!isFiniteRect(left[dimension]) || !isFiniteRect(right[dimension])) {
                continue;
            }
            const delta = Math.abs(left[dimension].top - right[dimension].top);
            if (delta > tolerance) {
                failures.push({ dimension, delta });
            }
        }
        return failures;
    }

    function visible(element, view) {
        if (!element) return false;
        const style = view.getComputedStyle(element);
        const rect = element.getBoundingClientRect();
        return style.display !== "none"
            && style.visibility !== "hidden"
            && rect.width > 0
            && rect.height > 0;
    }

    function rectOf(element) {
        if (!element) return null;
        const rect = element.getBoundingClientRect();
        return {
            top: rect.top,
            right: rect.right,
            bottom: rect.bottom,
            left: rect.left,
            width: rect.width,
            height: rect.height
        };
    }

    function identify(element, fallback) {
        if (!element) return fallback;
        if (element.id) return `#${element.id}`;
        const classes = [...element.classList].slice(0, 2);
        return classes.length
            ? `${element.tagName.toLowerCase()}.${classes.join(".")}`
            : `${element.tagName.toLowerCase()}[${fallback}]`;
    }

    function audit(options) {
        const settings = { ...DEFAULTS, ...(options || {}) };
        const documentRef = settings.document || root.document;
        if (!documentRef) {
            throw new Error("SUIAlignmentAudit requires a browser document.");
        }
        const view = documentRef.defaultView || root;
        const tolerance = Number(settings.tolerance);
        if (!Number.isFinite(tolerance) || tolerance < 0) {
            throw new Error("SUIAlignmentAudit tolerance must be a non-negative number.");
        }

        const failures = [];
        let comparisons = 0;
        const containers = [...documentRef.querySelectorAll(
            settings.containerSelector)];

        containers.forEach((container, containerIndex) => {
            const fields = [...container.querySelectorAll(settings.fieldSelector)]
                .filter((field) => field.parentElement === container)
                .filter((field) => visible(field, view))
                .map((field, fieldIndex) => {
                    const label = field.querySelector(settings.labelSelector);
                    const control = field.querySelector(settings.controlSelector);
                    return {
                        name: identify(field, `field-${fieldIndex + 1}`),
                        field: rectOf(field),
                        label: visible(label, view) ? rectOf(label) : null,
                        control: visible(control, view) ? rectOf(control) : null
                    };
                });

            for (let leftIndex = 0; leftIndex < fields.length; leftIndex += 1) {
                for (let rightIndex = leftIndex + 1;
                    rightIndex < fields.length;
                    rightIndex += 1) {
                    const pairFailures = comparePeerGeometry(
                        fields[leftIndex],
                        fields[rightIndex],
                        tolerance);
                    if (!areHorizontalPeers(
                        fields[leftIndex].field,
                        fields[rightIndex].field)) {
                        continue;
                    }
                    comparisons += 1;
                    pairFailures.forEach((failure) => failures.push({
                        container: identify(
                            container,
                            `container-${containerIndex + 1}`),
                        left: fields[leftIndex].name,
                        right: fields[rightIndex].name,
                        ...failure
                    }));
                }
            }
        });

        const report = {
            pass: failures.length === 0,
            tolerance,
            containers: containers.length,
            comparisons,
            failures
        };
        if (root.console) {
            const method = report.pass ? "info" : "error";
            root.console[method]("[SUI alignment audit]", report);
        }
        return report;
    }

    root.SUIAlignmentAudit = audit;
    if (typeof module !== "undefined" && module.exports) {
        module.exports = { audit, areHorizontalPeers, comparePeerGeometry };
    }
}(typeof window !== "undefined" ? window : globalThis));
