import { BlobServiceClient } from "@azure/storage-blob";
import { useEffect, useState } from "react";
import styles from "./Home.module.css";

const STORAGE_ACCOUNT_NAME = "mytubestoragecool";
const CONTAINER_NAME = "films";

async function fetchSasToken() {
  const response = await fetch(`https://localhost:7276/api/Tokens/container`);
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

const Home = () => {
  const [blobs, setBlobs] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    setLoading(true);

    const fetchData = async () => {
      let token = await fetchSasToken();

      setBlobs(await listBlobs(CONTAINER_NAME, token));
    };

    fetchData();
    setLoading(false);
  }, []); // Empty array to run the effect only once, when the component mounts.

  return (
    <div className={styles.container}>
      <h1>Blobs in the Azure Blob Storage Container:</h1>
      {loading ? (
        <div>Loading...</div>
      ) : (
        <ul>
          {!loading &&
            blobs.length > 0 &&
            blobs.map((blob, index) => <li key={index}>{blob}</li>)}
        </ul>
      )}
    </div>
  );
};

export default Home;
