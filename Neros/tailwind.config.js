/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    './Neros.Blazor/**/*.razor',
    './Neros.Blazor/**/*.cshtml',
    './Neros.Blazor/**/*.html'
  ],
  theme: {
    extend: {
      colors: {
        neros: {
          ink: '#071320',
          navy: '#0d1c2a',
          teal: '#138f7e',
          mint: '#dff8f3',
          line: '#dce4e9',
          amber: '#d3922e'
        }
      },
      fontFamily: {
        sans: ['Manrope', 'Aptos', 'Segoe UI', 'sans-serif'],
        display: ['Space Grotesk', 'Manrope', 'sans-serif']
      }
    }
  },
  plugins: []
};
