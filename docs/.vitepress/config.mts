import { defineConfig } from 'vitepress'

export default defineConfig({
    base: '/Rapp/',
    title: "Rapp",
    description: "Schema-Aware Binary Serialization for .NET",
    head: [
        ['link', { rel: 'icon', href: '/Rapp/logo.svg' }]
    ],
    ignoreDeadLinks: true,
    themeConfig: {
        logo: '/logo.svg',
        nav: [
            { text: 'Home', link: '/' },
            { text: 'Getting Started', link: '/GETTING_STARTED' },
            { text: 'Technical Summary', link: '/TECHNICAL_SUMMARY' },
            { text: 'API Reference', link: '/api' }
        ],

        sidebar: [
            {
                text: 'Introduction',
                items: [
                    { text: 'Getting Started', link: '/GETTING_STARTED' },
                    { text: 'Executive Summary', link: '/EXECUTIVE_SUMMARY' },
                    { text: 'Technical Summary', link: '/TECHNICAL_SUMMARY' },
                    { text: 'Migration Guide', link: '/MIGRATION_GUIDE' },
                    { text: 'Dependencies', link: '/DEPENDENCIES' }
                ]
            },
            {
                text: 'Advanced Features',
                items: [
                    { text: 'Ghost Reader', link: '/GHOST_READER' },
                    { text: 'Schema Evolution Demo', link: '/SCHEMA_EVOLUTION_DEMO' },
                    { text: 'Schema Evolution Technical', link: '/SCHEMA_EVOLUTION_TECHNICAL' },
                    { text: 'Telemetry', link: '/TELEMETRY' },
                    { text: 'Telemetry Quick Reference', link: '/TELEMETRY_QUICKREF' }
                ]
            }
        ],

        socialLinks: [
            { icon: 'github', link: 'https://github.com/Digvijay/Rapp' }
        ],

        footer: {
            message: 'Released under the MIT License.',
            copyright: 'Copyright © 2026 Digvijay'
        }
    }
})
