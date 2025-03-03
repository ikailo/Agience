import type { Config } from 'tailwindcss'
import formsPlugin from '@tailwindcss/forms'
//tailwindcss formsPlugin correct way for vite project: https://github.com/tailwindlabs/tailwindcss-forms/issues/151 by thecrypticace


const config: Config = {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  darkMode: 'class',
  theme: {
    extend: {},
  },
  plugins: [formsPlugin],
}

export default config 