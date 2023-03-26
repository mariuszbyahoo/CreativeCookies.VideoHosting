import { BlobServiceClient } from "@azure/storage-blob";
import { useEffect, useState } from "react";

const STORAGE_ACCOUNT_NAME = "mytubestoragecool";
const CONTAINER_NAME = "films";

const Home = (props) => {
  const [filmsList, setFilmsList] = useState([]);

  const fetchContainerSasToken = async () => {
    const response = await fetch("https://localhost:7276/api/tokens/container");
    const jsonResponse = await response.json();
    return jsonResponse.sasToken;
  };

  const getFilmList = async () => {
    const sasToken = await fetchContainerSasToken();
    const blobServiceClient = new BlobServiceClient(
      `https://${STORAGE_ACCOUNT_NAME}.blob.core.windows.net/?${sasToken}`
    );
    const containerClient =
      blobServiceClient.getContainerClient(CONTAINER_NAME);

    const films = [];
    for await (const blob of containerClient.listBlobsFlat()) {
      films.push({ name: blob.name, url: blob.url });
    }
    return films;
  };

  const getFilmUrlWithSasToken = async (filmBlobName) => {
    const sasToken = await fetchSasToken(filmBlobName);
    const blobServiceClient = new BlobServiceClient(
      `https://${STORAGE_ACCOUNT_NAME}.blob.core.windows.net/?${sasToken}`
    );
    const containerClient =
      blobServiceClient.getContainerClient(CONTAINER_NAME);
    const blobClient = containerClient.getBlobClient(filmBlobName);

    return blobClient.url;
  };

  const fetchSasToken = async (blobName) => {
    const response = await fetch(
      `https://localhost:7276/api/tokens/blob/${encodeURIComponent(blobName)}`
    );
    const jsonResponse = await response.json();
    return jsonResponse.sasToken;
  };

  const filmList = getFilmList();
  setFilmsList(filmList);

  return (
    <>
      <div style={{ textAlign: "center" }}>
        <h1>Below the films list:</h1>
        <ul>
          {filmsList.length > 0 &&
            filmsList.map((filmObj) => {
              return (
                <li>
                  Name: {filmObj.name} | URL: {filmObj.url}
                </li>
              );
            })}
        </ul>
      </div>
    </>
  );
};

export default Home;
