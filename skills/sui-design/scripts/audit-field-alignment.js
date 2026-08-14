(function (root) {
    "use strict";

    const DEFAULTS = Object.freeze({
        containerSelector: "[data-sui-align-row]",
        fieldSelector:
            ":scope > [data-sui-align-field], :scope > .field-group, :scope > .sui-field, :scope > .mud-input-control",
        labelSelector:
            ":scope > label, :scope > legend, :scope > [data-sui-field-label], .sui-field__label, .mud-input-label",
        controlSelector:
            ":scope > input, :scope > select, :scope > textarea, :scope > [data-sui-field-control], .sui-field__input, .sui-select__trigger, .mud-input, .mud-select-input, .mud-picker-input-button",
        tolerance: 2,
        requireContainers: true,
        requireComparisons: true,
        requireControls: true,
        requireLabels: false
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

    function measurePeerGeometry(left, right) {
        if (!areHorizontalPeers(left.field, right.field)) return [];
        const measurements = [];
        for (const dimension of ["field", "label", "control"]) {
            if (!isFiniteRect(left[dimension]) || !isFiniteRect(right[dimension])) {
                continue;
            }
            const delta = Math.abs(left[dimension].top - right[dimension].top);
            measurements.push({ dimension, metric: "top", delta });
        }
        if (isFiniteRect(left.control) && isFiniteRect(right.control)) {
            const heightDelta = Math.abs(
                left.control.height - right.control.height);
            measurements.push({
                dimension: "control",
                metric: "height",
                delta: heightDelta
            });
        }
        return measurements;
    }

    function comparePeerGeometry(left, right, tolerance) {
        return measurePeerGeometry(left, right)
            .filter((measurement) => measurement.delta > tolerance)
            .map((measurement) => ({
                reason: "misaligned",
                ...measurement
            }));
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

    function identifyField(element, control, fallback) {
        const explicitName = element?.dataset?.suiAlignName?.trim();
        if (explicitName) return explicitName;
        if (element?.id) return `#${element.id}`;
        if (control?.id) return `#${control.id}`;
        return identify(element, fallback);
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
        const diagnostics = [];
        const pairs = [];
        let comparisons = 0;
        let measuredFields = 0;
        const containers = [...documentRef.querySelectorAll(
            settings.containerSelector)];

        if (settings.requireContainers && containers.length === 0) {
            failures.push({ reason: "no-containers" });
        }

        containers.forEach((container, containerIndex) => {
            const containerName = identify(
                container,
                `container-${containerIndex + 1}`);
            const fields = [...container.querySelectorAll(settings.fieldSelector)]
                .filter((field) => field.parentElement === container)
                .filter((field) => visible(field, view))
                .map((field, fieldIndex) => {
                    const label = field.querySelector(settings.labelSelector);
                    const control = field.querySelector(settings.controlSelector);
                    return {
                        name: identifyField(
                            field,
                            control,
                            `field-${fieldIndex + 1}`),
                        field: rectOf(field),
                        label: visible(label, view) ? rectOf(label) : null,
                        control: visible(control, view) ? rectOf(control) : null
                    };
                });
            measuredFields += fields.length;

            fields.forEach((field) => {
                if (!field.label) {
                    diagnostics.push({
                        reason: "missing-label",
                        container: containerName,
                        field: field.name
                    });
                    if (settings.requireLabels) {
                        failures.push(diagnostics.at(-1));
                    }
                }
                if (!field.control) {
                    diagnostics.push({
                        reason: "missing-control",
                        container: containerName,
                        field: field.name
                    });
                    if (settings.requireControls) {
                        failures.push(diagnostics.at(-1));
                    }
                }
            });

            for (let leftIndex = 0; leftIndex < fields.length; leftIndex += 1) {
                for (let rightIndex = leftIndex + 1;
                    rightIndex < fields.length;
                    rightIndex += 1) {
                    if (!areHorizontalPeers(
                        fields[leftIndex].field,
                        fields[rightIndex].field)) {
                        continue;
                    }
                    comparisons += 1;
                    const measurements = measurePeerGeometry(
                        fields[leftIndex],
                        fields[rightIndex]);
                    pairs.push({
                        container: containerName,
                        left: fields[leftIndex].name,
                        right: fields[rightIndex].name,
                        measurements
                    });
                    const pairFailures = measurements
                        .filter((measurement) => measurement.delta > tolerance)
                        .map((measurement) => ({
                            reason: "misaligned",
                            ...measurement
                        }));
                    pairFailures.forEach((failure) => failures.push({
                        container: containerName,
                        left: fields[leftIndex].name,
                        right: fields[rightIndex].name,
                        ...failure
                    }));
                }
            }
        });

        if (settings.requireComparisons && comparisons === 0) {
            failures.push({ reason: "no-horizontal-comparisons" });
        }

        const report = {
            pass: failures.length === 0,
            tolerance,
            containers: containers.length,
            measuredFields,
            comparisons,
            pairs,
            failures,
            diagnostics
        };
        if (root.console) {
            const method = report.pass ? "info" : "error";
            root.console[method]("[SUI alignment audit]", report);
        }
        return report;
    }

    root.SUIAlignmentAudit = audit;
    if (typeof module !== "undefined" && module.exports) {
        module.exports = {
            audit,
            areHorizontalPeers,
            measurePeerGeometry,
            comparePeerGeometry
        };
    }
}(typeof window !== "undefined" ? window : globalThis));
