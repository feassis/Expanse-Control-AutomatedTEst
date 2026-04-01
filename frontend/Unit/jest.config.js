module.exports = {
  preset: 'ts-jest',
  testEnvironment: 'jsdom',
  setupFilesAfterEnv: ['<rootDir>/jest.setup.ts'],
  moduleNameMapper: {
    '^@/(.*)$': '<rootDir>/../../../ExameDesenvolvedorDeTestes/web/src/$1',
    '^react$': '<rootDir>/../../../ExameDesenvolvedorDeTestes/web/node_modules/react',
    '^react-dom$': '<rootDir>/../../../ExameDesenvolvedorDeTestes/web/node_modules/react-dom',
    '^react/jsx-runtime$': '<rootDir>/../../../ExameDesenvolvedorDeTestes/web/node_modules/react/jsx-runtime',
  },
};