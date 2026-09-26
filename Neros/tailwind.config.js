/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    './Neros.Blazor/**/*.razor',
    './Neros.Blazor/**/*.cshtml',
    './Neros.Blazor/**/*.html'
  ],
  theme: {
    extend: {
      fontFamily: {
        sans: ['Manrope', 'Aptos', 'Segoe UI', 'sans-serif']
      }
    }
  },
  plugins: []
};
