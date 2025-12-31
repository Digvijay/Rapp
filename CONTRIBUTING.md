# Contributing to Rapp

Thank you for your interest in contributing to Rapp! This document provides guidelines and information for contributors.

## 🚀 Quick Start

1. Fork the repository
2. Clone your fork: `git clone https://github.com/your-username/Rapp.git`
3. Create a feature branch: `git checkout -b feature/your-feature-name`
4. Make your changes
5. Run tests: `dotnet test`
6. Submit a pull request

## 🏗️ Development Setup

### Prerequisites
- .NET 10.0 SDK
- Git

### Building from Source

```bash
# Clone the repository
git clone https://github.com/Digvijay/Rapp.git
cd Rapp

# Restore dependencies
dotnet restore

# Build all projects
dotnet build

# Run tests
dotnet test

# Run the demo
cd src/Rapp.Playground
dotnet run
```

## 📋 Development Guidelines

### Code Style
- Follow C# coding conventions
- Use meaningful variable and method names
- Add XML documentation comments to public APIs
- Keep methods focused and single-purpose

### Testing
- Write unit tests for new features
- Ensure all tests pass before submitting PR
- Test both success and failure scenarios
- Include performance tests for serialization changes

### Commit Messages
- Use clear, descriptive commit messages
- Start with a verb (Add, Fix, Update, etc.)
- Reference issue numbers when applicable

Example:
```
Add schema validation for property reordering

- Implement hash-based schema detection
- Add cache invalidation on schema mismatch
- Update tests for new validation logic

Closes #123
```

## 🐛 Reporting Issues

When reporting bugs, please include:
- .NET version and OS
- Steps to reproduce
- Expected vs. actual behavior
- Code samples if applicable

## 💡 Feature Requests

We welcome feature requests! Please:
- Check existing issues first
- Provide clear use case description
- Explain the business value
- Consider implementation complexity

## 📚 Documentation

- Update README.md for new features
- Add XML documentation comments
- Update examples and demos as needed
- Keep the schema evolution demo current

## 🔄 Pull Request Process

1. Ensure your code builds and tests pass
2. Update documentation if needed
3. Add tests for new functionality
4. Ensure PR description clearly describes the changes
5. Wait for review and address feedback

## 📞 Getting Help

- **Issues**: For bugs and feature requests
- **Discussions**: For questions and general discussion
- **Documentation**: Check the `/docs` folder first

## 🎉 Recognition

Contributors will be recognized in release notes and the contributor list. Thank you for helping make Rapp better!

## 📄 License

By contributing to Rapp, you agree that your contributions will be licensed under the MIT License.