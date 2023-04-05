import { BlobServiceClient } from "@azure/storage-blob";
import { useCallback, useEffect, useState } from "react";
import styles from "./FilmsList.module.css";
import Mosaic from "./Mosaic";

const FilmsList = () => {
  const [filmBlobs, setFilmBlobs] = useState([]);
  const [thumbnailBlobs, setThumbnailBlobs] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState();

  const fetchMoviesHandler = useCallback(() => {
    setLoading(true);
    setError(null);
    fetchSasToken()
      .then((token) => {
        listBlobs(process.env.REACT_APP_CONTAINER_NAME, token)
          .then((blobs) => {
            setFilmBlobs(blobs.filter((b) => b.name.includes(".mp4")));
            setThumbnailBlobs(
              blobs.filter((b) => b.name.includes(".png")).map((b) => b.name)
            );
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
    console.log(
      `process.env.REACT_APP_API_ADDRESS: ${process.env.REACT_APP_API_ADDRESS}`
    );
    const response = await fetch(
      `https://${process.env.REACT_APP_API_ADDRESS}/api/SAS/container/`
    );
    const data = await response.json();
    return data.sasToken;
  }

  async function listBlobs(containerName, sasToken) {
    const blobServiceClient = new BlobServiceClient(
      `https://${process.env.REACT_APP_STORAGE_ACCOUNT_NAME}.blob.core.windows.net/?${sasToken}`
    );

    const containerClient = blobServiceClient.getContainerClient(containerName);
    const blobs = [];
    for await (const blob of containerClient.listBlobsFlat()) {
      blobs.push(blob);
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

  if (filmBlobs.length > 0) {
    // Order by date desc
    filmBlobs.sort(
      (a, b) =>
        new Date(b.properties.createdOn) - new Date(a.properties.createdOn)
    );
    content = <Mosaic filmBlobs={filmBlobs} thumbnailBlobs={thumbnailBlobs} />;
  }

  return (
    <div className={styles.container}>
      <h1>Blobs in the Azure Blob Storage Container:</h1>
      {content}
    </div>
  );
};

export default FilmsList;
