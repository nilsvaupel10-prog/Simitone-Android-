/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        sims: {
          blue: '#1b3a6b',
          lightBlue: '#3b82f6',
          cyan: '#00c6ff',
          green: '#22c55e',
          plum: '#20d52b',
          panel: 'rgba(18, 28, 48, 0.92)',
          dark: '#0f172a',
        }
      },
      fontFamily: {
        comic: ['"Comic Sans MS"', '"Chalkboard SE"', 'cursive', 'sans-serif'],
      }
    },
  },
  plugins: [],
}
