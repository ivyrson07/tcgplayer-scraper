import logo from './logo.svg';
import './App.css';

import { Counter, Display } from './components';

import { DataContextProvider } from './context/DataContext';

function App() {
  return (
    <div className="App">
      <header className="App-header">
        <img src={logo} className="App-logo" alt="logo" />

        <DataContextProvider>
          <Display />
          <Counter />
        </DataContextProvider>

      </header>
    </div>
  );
}

export default App;
