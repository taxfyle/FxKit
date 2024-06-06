import {defineConfig} from 'vitepress'

// https://vitepress.dev/reference/site-config
export default defineConfig({
  title: "FxKit",
  lang: "en-US",
  description: "Functional programming utilities and Roslyn components for writing expressive C#",
  lastUpdated: true,

  head: [
    ['link', { rel: "apple-touch-icon", sizes: "180x180", href: "/FxKit/apple-touch-icon.png" }],
    ['link', { rel: "icon", type: "image/png", sizes: "32x32", href: "/FxKit/favicon-32x32.png" }],
    ['link', { rel: "icon", type: "image/png", sizes: "16x16", href: "/FxKit/favicon-16x16.png" }],
    ['link', { rel: "manifest", href: "/FxKit/manifest.json" }],
    ['meta', { property: 'og:title', content: 'FxKit Documentation' }],
    ['meta', { property: 'og:type', content: 'website' }],
    ['meta', {
      property: 'og:description',
      content: 'Functional programming utilities and Roslyn components for writing expressive C#'
    }],
    ['meta', { property: 'og:url', content: 'https://taxfyle.github.io/FxKit/' }]
  ],

  themeConfig: {
    // https://vitepress.dev/reference/default-theme-config
    nav: [
      { text: 'Home', link: '/' },
      { text: 'Intro', link: '/introduction' }
    ],

    search: {
      provider: 'local'
    },

    outline: {
      level: [2, 3]
    },

    logo: '/favicon-128x128.png',

    sidebar: [
      {
        text: 'Introduction',
        items: [
          { text: 'What is FxKit', link: '/introduction' },
          { text: 'Getting Started', link: '/getting-started' }
        ]
      },
      {
        text: 'Core',
        items: [
          { text: 'Data Types', link: '/core/' },
          { text: 'Unit', link: '/core/unit' },
          { text: 'Option', link: '/core/option' },
          { text: 'Result', link: '/core/result' },
          { text: 'Validation', link: '/core/validation' },
          { text: 'Transformer Methods', link: '/core/transformer' }
        ]
      },
      {
        text: 'Compiler Services',
        items: [
          { text: 'Overview', link: '/compiler/' },
          { text: 'EnumMatch Generator', link: '/compiler/enum-match' },
          { text: 'Lambda Generator', link: '/compiler/lambda' },
          { text: 'Union Generator', link: '/compiler/union' }
        ]
      },
      {
        text: 'Annotations',
        items: [
          { text: 'Overview', link: '/annotations/' }
        ]
      },
      {
        text: 'Testing',
        items: [
          { text: 'Overview', link: '/testing/' }
        ]
      }
    ],

    socialLinks: [
      { icon: 'github', link: 'https://github.com/taxfyle/FxKit' }
    ],
    footer: {
      message: 'Released under the MIT License.',
      copyright: 'Copyright © Taxfyle and contributors.'
    },
    editLink: {
      pattern: 'https://github.com/taxfyle/FxKit/edit/main/docs/:path',
      text: 'Edit this page on GitHub'
    }
  },
  base: '/FxKit/',
  sitemap: {
    hostname: "https://taxfyle.github.io/FxKit/"
  }
})
