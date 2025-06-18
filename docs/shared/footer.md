## 🤝 Contributing

We welcome contributions! Here's how to get started:

1. **Fork** the repository
2. **Create** your feature branch: `git checkout -b feature/amazing-feature`
3. **Commit** your changes: `git commit -m 'Add amazing feature'`
4. **Push** to the branch: `git push origin feature/amazing-feature`
5. **Open** a Pull Request

### Development Setup
```bash
git clone <repository-url>
cd MergeIncludes
dotnet restore
dotnet build
dotnet test
```

### Building Documentation
```bash
# Update this README from modular source
cd docs
MergeIncludes ./README-template.md -o ../README.md

# Watch mode for documentation development
MergeIncludes ./README-template.md --watch -o ../README.md
```

---

## 📝 License

This project is licensed under the **GPL-3.0 License** - see the [LICENSE](LICENSE) file for details.

---

## 🆘 Support & Community

- 🐛 **Bug Reports**: [GitHub Issues](../../issues)
- 💡 **Feature Requests**: [GitHub Discussions](../../discussions)
- 📚 **Documentation**: [Wiki](../../wiki)
- 💬 **Community**: [Discussions](../../discussions)

---

<div align="center">

**⭐ Star this repo if MergeIncludes improves your workflow! ⭐**

*Built with ❤️ using .NET 9 and modern C# practices*

**This README was assembled using MergeIncludes itself - a self-demonstrating example! �✨**

**[Download Latest Release](../../releases) • [View Documentation](../../wiki) • [Join Community](../../discussions)**

</div>
