# Linear progress accessibility values

Provisioning discoveries exercised a fractional lifetime in a pt-BR render. The progress bar exposed a comma decimal in `aria-valuenow`. ARIA needs a machine-readable decimal, so the renderer now uses the invariant culture for determinate values. Indeterminate progress still omits the current value.

The pt-BR regression test and all 1,247 library tests passed. The consumer's Chromium fixture read the numeric value at 390 px and 1280 px in both themes.
