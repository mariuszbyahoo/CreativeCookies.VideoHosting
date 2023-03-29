import { BlobServiceClient } from "@azure/storage-blob";
import { useCallback, useEffect, useState } from "react";
import styles from "./Home.module.css";

const STORAGE_ACCOUNT_NAME = "mytubestoragecool";
const CONTAINER_NAME = "films";

const Home = () => {
  const [blobs, setBlobs] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState();

  const fetchMoviesHandler = useCallback(() => {
    setLoading(true);
    setError(null);
    fetchSasToken()
      .then((token) => {
        listBlobs(CONTAINER_NAME, token)
          .then((blobs) => {
            setBlobs(blobs);
            setLoading(false);
          })
          .catch((error) => {
            setError(error);
            setLoading(false);
          });
      })
      .catch((error) => {
        setError(error);
        setLoading(false);
      });
  }, []);

  useEffect(() => {
    fetchMoviesHandler();
  }, [fetchMoviesHandler]); // Empty array to run the effect only once, when the component mounts.

  async function fetchSasToken() {
    const response = await fetch(
      `https://localhost:7276/api/Tokens/container/`
    );
    const data = await response.json();
    return data.sasToken;
  }

  async function listBlobs(containerName, sasToken) {
    const blobServiceClient = new BlobServiceClient(
      `https://${STORAGE_ACCOUNT_NAME}.blob.core.windows.net/?${sasToken}`
    );

    const containerClient = blobServiceClient.getContainerClient(containerName);
    const blobs = [];
    for await (const blob of containerClient.listBlobsFlat()) {
      blobs.push(blob.name);
    }
    return blobs;
  }

  let content = <p>Upload a movie to get started!</p>;
  if (loading) {
    content = <p>Loading...</p>;
  }
  if (error) {
    content = <h4>An error occured, while fetching the API: {error}</h4>;
  }
  if (blobs.length > 0) {
    content = (
      <ul>
        {blobs.map((blob, index) => (
          <li key={index}>{blob}</li>
        ))}
      </ul>
    );
  }

  return (
    <div className={styles.container}>
      <h1>Blobs in the Azure Blob Storage Container:</h1>
      {content}
    </div>
  );
};

export default Home;
