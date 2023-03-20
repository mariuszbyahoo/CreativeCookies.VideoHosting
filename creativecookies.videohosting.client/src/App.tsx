import React from 'react';
import logo from './logo.svg';
import './App.css';
import PlyrComponent from './Plyr';
import { BrowserRouter, Route, Routes } from 'react-router-dom';
import HomeComponent from './Home';
import BlobTypeListComponent from './BlobTypeList';
import Layout from './Layout';

function App() {
    return (
      <BrowserRouter>
        <Routes>
          <Route path="/" element={<Layout />}>
            <Route index element={<HomeComponent />} />
            <Route path="blobsList" element={<BlobTypeListComponent connectionString='' containerName='' />} />
            <Route path="plyr" element={<PlyrComponent />} />
            <Route path="*" element={<><h4>Wrong path</h4></>} />
          </Route>
        </Routes>
      </BrowserRouter>
    );
}

export default App;
