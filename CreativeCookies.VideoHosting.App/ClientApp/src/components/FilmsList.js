import { BlobServiceClient } from "@azure/storage-blob";
import { useCallback, useEffect, useState } from "react";
import styles from "./FilmsList.module.css";
import Mosaic from "./Mosaic";
import {
  FormControl,
  IconButton,
  InputAdornment,
  InputLabel,
  MenuItem,
  OutlinedInput,
  Select,
  TextField,
} from "@mui/material";
import { Search } from "@mui/icons-material";

const FilmsList = () => {
  const [filmBlobs, setFilmBlobs] = useState([]);
  const [filteredFilmBlobs, setFilteredFilmBlobs] = useState([]);
  const [thumbnailBlobs, setThumbnailBlobs] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState();

  const fetchMoviesHandler = useCallback(() => {
    setLoading(true);
    setError(null);
    fetchSasToken()
      .then((token) => {
        listBlobs(process.env.REACT_APP_FILMS_CONTAINER_NAME, token)
          .then((blobs) => {
            const filmBlobs = blobs.filter((b) => b.name.includes(".mp4"));
            setFilmBlobs(filmBlobs);
            setFilteredFilmBlobs(filmBlobs); // Set the filteredFilmBlobs here
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
    const response = await fetch(
      `https://${process.env.REACT_APP_API_ADDRESS}/api/SAS/filmsList/`
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
      const blockBlobClient = containerClient.getBlockBlobClient(blob.name);
      const propertiesResponse = await blockBlobClient.getProperties();
      blob.metadata = propertiesResponse.metadata;
      blobs.push(blob);
    }
    return blobs;
  }

  const filterInputChangeHandler = (e) => {
    setFilteredFilmBlobs(
      filmBlobs.filter((b) =>
        b.name.toLowerCase().includes(e.target.value.toLowerCase())
      )
    ); // Update the filteredFilmBlobs instead
  };

  let content = <p>Upload a movie to get started!</p>;
  if (loading) {
    content = <p>Loading...</p>;
  }
  if (error) {
    content = <h4>An error occured, while fetching the API: {error}</h4>;
  }

  if (filteredFilmBlobs.length > 0) {
    // Order by date desc
    filteredFilmBlobs.sort(
      (a, b) =>
        new Date(b.properties.createdOn) - new Date(a.properties.createdOn)
    );
    content = (
      <Mosaic filmBlobs={filteredFilmBlobs} thumbnailBlobs={thumbnailBlobs} />
    );
  }

  return (
    <div className={styles.container}>
      <div className="row">
        <FormControl variant="standard" sx={{ m: 1, minWidth: 120 }}>
          <TextField
            label="Filter"
            id="filter-search"
            onChange={filterInputChangeHandler}
            InputProps={{
              startAdornment: (
                <InputAdornment position="start">
                  <Search />
                </InputAdornment>
              ),
            }}
            variant="filled"
          />
        </FormControl>
      </div>
      {content}
    </div>
  );
};

export default FilmsList;
