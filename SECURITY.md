# Security Policy

## Supported Versions

We release patches for security vulnerabilities for the following versions:

| Version | Supported          |
| ------- | ------------------ |
| 1.x.x   | :white_check_mark: |
| < 1.0   | :x:                |

## Reporting a Vulnerability

**Please do not report security vulnerabilities through public GitHub issues.**

Instead, please report security vulnerabilities by emailing:

digvijay atr digvijay.dev

You should receive a response within 48 hours. If for some reason you do not, please follow up via email to ensure we received your original message.

Please include the following information (as much as you can provide) to help us better understand the nature and scope of the possible issue:

* Type of issue (e.g. buffer overflow, schema validation bypass, injection, etc.)
* Full paths of source file(s) related to the manifestation of the issue
* The location of the affected source code (tag/branch/commit or direct URL)
* Any special configuration required to reproduce the issue
* Step-by-step instructions to reproduce the issue
* Proof-of-concept or exploit code (if possible)
* Impact of the issue, including how an attacker might exploit the issue

This information will help us triage your report more quickly.

## Preferred Languages

We prefer all communications to be in English.

## Security Update Policy

When we learn of a security vulnerability, we will:

1. Confirm the problem and determine affected versions
2. Audit code to find any similar problems
3. Prepare fixes for all still-supported releases
4. Release patched versions as soon as possible

## Security-Related Configuration

### Telemetry and Logging

Rapp collects minimal telemetry by default. To disable:

```csharp
RappConfiguration.EnableTelemetry = false;
```

### Detailed Errors

Detailed errors are disabled by default to prevent information leakage. Only enable in development:

```csharp
// Development only
RappConfiguration.EnableDetailedErrors = true;
```

### Schema Validation

Rapp performs cryptographic schema validation on all deserialization operations. This cannot be disabled and protects against:

* Accidental deserialization of incompatible data
* Schema evolution bugs during deployments
* Potential injection attacks via malformed cache data

## Known Limitations

### Denial of Service

Large payloads may consume significant memory during deserialization. Applications should implement appropriate size limits at the cache layer.

### Side-Channel Attacks

Schema hash comparison uses standard equality checks and may be vulnerable to timing attacks. However, schema hashes are not considered secret material.

## Disclosure Policy

When we receive a security bug report, we will:

1. Confirm the problem and determine affected versions
2. Audit code for any similar problems
3. Prepare fixes for all still-supported releases
4. Publish a security advisory on GitHub
5. Release new versions with fixes

## Comments on this Policy

If you have suggestions on how this process could be improved, please submit a pull request.
