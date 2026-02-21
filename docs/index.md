---
layout: home

hero:
  name: Rapp
  text: Schema-Aware Binary Serialization for .NET
  tagline: Bridging the gap between MemoryPack's raw performance and enterprise deployment safety requirements.
  actions:
    - theme: brand
      text: Get Started
      link: /GETTING_STARTED
    - theme: alt
      text: View on GitHub
      link: https://github.com/Digvijay/Rapp

features:
  - title: Blazing Fast
    details: Leverages source generators and MemoryPack for a 4.4x serialization and 17.6x deserialization speedup over JSON caching, without relying on reflection.
  - title: Safe Schema Evolution
    details: Automatically detects schema changes and computes cryptographic validation hashes so you never experience deployment crashes with breaking schema iterations.
  - title: Native AOT Ready
    details: 100% compatible with Native AOT compilation, allowing you to use it smoothly with Serverless and high-performance Cloud functions.
  - title: Ghost Reader
    details: A Zero-Copy, Zero-Allocation ref struct view capability for absolute low-latency reads.
---
