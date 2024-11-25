module.exports = {
  ci: {
    collect: {
      numberOfRuns: 1,
      startServerCommand: 'npm run start', // If you need to start your server
      url: [
        'https://cloudstrucc.com/',
        'https://cloudstrucc.com/about',
        // Add other URLs you want to test
      ],
      settings: {
        preset: 'desktop'
      }
    },
    assert: {
      assertions: {
        'categories:accessibility': ['error', {minScore: 0.9}],
        'categories:best-practices': ['error', {minScore: 0.9}],
        'categories:performance': ['error', {minScore: 0.8}]
      }
    },
    upload: {
      target: 'filesystem',
      outputDir: './lighthouse-results',
      reportFilenamePattern: "%%PATHNAME%%-%%DATETIME%%-report.%%EXTENSION%%"
    }
  }
};