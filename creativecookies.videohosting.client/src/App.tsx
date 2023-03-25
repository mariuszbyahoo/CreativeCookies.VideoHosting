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
                    <Route path="blobsList" element={
                        <BlobTypeListComponent
                            connectionString='?sv=2021-08-06&ss=btqf&srt=sco&st=2023-03-11T19%3A02%3A42Z&se=2023-04-12T18%3A02%3A00Z&sp=rl&sig=YtpkB3f8khUQryZ0PeriBtVc4KQxPwyFbbL94lkCn0w%3D'
                            containerName='mytubestoragecool' />} />
            <Route path="plyr" element={<PlyrComponent />} />
            <Route path="*" element={<><h4>Wrong path</h4></>} />
          </Route>
        </Routes>
      </BrowserRouter>
    );
}

export default App;
