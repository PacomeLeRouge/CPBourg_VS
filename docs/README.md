# Maintainer documentation

This directory describes the implementation of the CPBourg NextGen Operator
Interface prototype. It is for developers, reviewers, and future integration
teams. Operator instructions belong in the separate Operator User Guide.

The documentation reflects application version **1.30.0**. The application is
an interactive prototype: it does not control production equipment and it does
not currently connect to the WFM.

## Start here

| Document | Use it when |
|---|---|
| [Architecture](ARCHITECTURE.md) | Understanding ownership, dependencies, navigation, and the code map |
| [State and workflows](STATE_AND_WORKFLOWS.md) | Changing run simulation, jobs, STFO transactions, settings, errors, or machine-line behavior |
| [Development](DEVELOPMENT.md) | Setting up, building, debugging, publishing, or changing the code |
| [Localization and units](LOCALIZATION_AND_UNITS.md) | Adding UI text, languages, measurements, or display preferences |
| [WFM integration](WFM_INTEGRATION.md) | Replacing local simulations with approved backend services |
| [Testing](TESTING.md) | Running the current checks or adding automated tests |

## Documentation rules

- Document current behavior, not an unimplemented design, unless a section is
  explicitly marked **Proposed**.
- Treat values in millimeters as canonical application data. Unit selection is
  a presentation concern.
- Label every simulated machine action and sample-data source.
- Update the relevant workflow guide in the same change as behavior.
- Add XML comments to public contracts and non-obvious invariants; avoid
  comments that merely restate a method name.
- Keep release history in the root README and release notes. Keep durable
  design knowledge in this directory.

## Prototype boundary

The authoritative release boundary is [Known limitations](../KNOWN_LIMITATIONS.md).
In particular, Start, Pause, Stop, Purge, alerts, machine states, speeds, the
technician code, and the WFM connection are local demonstrations. Any real
machine integration requires an approved protocol implementation, security and
safety review, fault handling, and system acceptance testing.
