# Card header mobile actions

1. Completed — reproduce actual consumer header squeezing the title beside avatar/actions at 390px; browser assertion fails before correction.
2. Completed — let native header actions follow copy on mobile, using the existing layout and spacing tokens.
3. In progress — validate library and actual consumer preview at 390/768/1280, both themes; record evidence and deliver main.

Reference: shared SUI migration preview, existing SUICardHeader anatomy. Preserve wide-screen behavior, slots and action labels. No application-local override for this reusable rule.

Consumer browser regression failed before the CSS correction and passed afterwards at 390/768/1280 in light/dark. The title retains more than half the phone width; actions follow title/copy on a new row. Existing 1,230 library tests passed.
