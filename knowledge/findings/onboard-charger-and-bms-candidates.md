# Onboard charger and BMS candidates

## Description

Candidates found on 2026-11-02 for [ADR 0017](../../docs/adr/0017-li-ion-battery-charged-on-board.md)'s battery (3S1P 18650 Li-ion). Requirements: USB-C PD input that a wireless receiver can also feed later, charging while the robot stays on, charge end at 4.1 V per cell, and a BMS that allows the servos' ~25 A peaks. Nothing here was bought or tested; ratings come from vendor pages and, where marked, from seller listings or third-party summaries.

| Candidate | Kind | What was found | Open question |
|---|---|---|---|
| TI USB-PD-CHG-EVM-01 (TPS25750 + BQ25792) | Evaluation board | Official TI board: USB PD controller with a 5 A buck-boost 1–4S charger, NVDC power path, dual input | Charge settings are written over I²C, so 4.1 V needs a host; fits revision D's onboard computer |
| TI TPS25750EVM (TPS25750 + BQ25731) | Evaluation board | TI user guide: sinks 5–20 V PD and charges a 3S battery | Same need for a host to configure it |
| BQ25798 (TI) | Charger IC | 1–4S buck-boost, 3.6–24 V input, 5 A charge, NVDC, dual input | Needs a carrier board; battery FET current rating not found |
| IP2369 module (Injoinic) | Standalone module | Charge-only, 3S–6S; a third-party summary gives 4.1 V per cell at a 6.2 kΩ setting resistor, 4.2 V at 9.1 kΩ | No official datasheet found; module quality varies by seller |
| IP2368 module (Injoinic) | Standalone module | Bidirectional 100 W USB-C PD; seller listings only | 4.1 V setting not confirmed; it also acts as a power bank output, which is not wanted |
| Generic 3S 40 A balanced BMS | BMS | Common, cheap, fixed thresholds | Overcurrent trip level and delay against servo inrush are rarely stated |
| Daly or JBD 3S smart BMS | BMS | Bluetooth, configurable thresholds; JBD's 3–4S boards found were 80–100 A | Larger than needed; check size |

## Why It Matters

Every charger found is rated for a few amperes on its system output, an order of magnitude below the servos' peaks. The motors therefore draw from the battery side of the BMS, not through the charger. A charger also ends a charge by watching its current fall, which a running load hides, so charging assumes the motor bus is switched off. This also shapes the wiring diagram for revision C.

## Evidence

- [TI USB-PD-CHG-EVM-01](https://www.ti.com/tool/USB-PD-CHG-EVM-01), [TPS25750EVM user guide](https://ti.com/lit/slvuby2), [BQ25798 product page](https://www.ti.com/product/de-de/BQ25798)
- [IP2369 summary (done.land)](https://done.land/components/power/powersupplies/battery/chargers/charge-discharge/ip2369/), [IP2368 seller listing](https://www.onbuy.com/ro/p/ip2368-bidirectional-100w-fast-charging-module-buck-boost-type-c-interface-4s-lithium-battery-high-power-charging-board~p252223345/)
- [Generic 3S 40 A BMS listing](https://circuitmall.ng/product/3s-40a-lithium-battery-management-system-balanced/), [JBD smart BMS range](https://robu.in/product-category/jiabaida-jbd-smart-battery-management-system/)
- XL430-W250 current figures: [Robotis e-Manual](https://emanual.robotis.com/docs/en/dxl/x/xl430-w250/)

## Recommendation

For revision C, without an onboard computer: a standalone charge-only module (IP2369 class) set to 4.1 V, and a 3S balanced BMS rated 40 A, after confirming its overcurrent delay. Reconsider a TI board in revision D, when the computer can configure it. Before buying, get the module's own datasheet or schematic for the 4.1 V setting.

## Related Components

[hardware/mayday](../../hardware/mayday/README.md)

## Related Decisions

[ADR 0017](../../docs/adr/0017-li-ion-battery-charged-on-board.md)
