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
      const blockBlobClient = containerClient.getBlockBlobClient(blob.name);
      const propertiesResponse = await blockBlobClient.getProperties();
      blob.metadata = propertiesResponse.metadata;
      blobs.push(blob);
    }
    return blobs;
  }

  const filterInputChangeHandler = (e) => {
    console.log(e.target.value);
  };

  const filterByChangeHandler = (e) => {
    console.log(e.target.value);
  };

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
      <div className="row">
        <div className="col-6">
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
        <div className="col-6">
          <FormControl variant="standard" sx={{ m: 1, minWidth: 120 }}>
            <InputLabel id="demo-simple-select-standard-label">Age</InputLabel>
            <Select
              labelId="demo-simple-select-standard-label"
              id="demo-simple-select-standard"
              value={20}
              onChange={filterByChangeHandler}
              label="Age"
            >
              <MenuItem value="">
                <em>None</em>
              </MenuItem>
              <MenuItem value={10}>Ten</MenuItem>
              <MenuItem value={20}>Twenty</MenuItem>
              <MenuItem value={30}>Thirty</MenuItem>
            </Select>
          </FormControl>
        </div>
      </div>
      {content}
    </div>
  );
};

export default FilmsList;
