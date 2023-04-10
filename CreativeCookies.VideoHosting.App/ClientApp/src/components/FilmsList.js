﻿import { BlobServiceClient } from "@azure/storage-blob";
import { useCallback, useEffect, useState } from "react";
import styles from "./FilmsList.module.css";
import Mosaic from "./Mosaic";
import {
  Button,
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
  const [thumbnailBlobsNames, setThumbnailBlobNames] = useState([]);
  const [pageNumber, setPageNumber] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [searchTerm, setSearchTerm] = useState("");
  const [hasMore, setHasMore] = useState(true);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState();

  const fetchMoviesHandler = useCallback(async () => {
    if (!hasMore) return; // Don't fetch if there are no more items

    setLoading(true);
    setError(null);

    fetchSasToken()
      .then((token) => {
        fetch(
          `https://${process.env.REACT_APP_API_ADDRESS}/api/blobs/films?search=&pageNumber=${pageNumber}&pageSize=24`
        )
          .then((response) => response.json())
          .then((data) => {
            console.log("data: ", data);
            setFilmBlobs((prevFilmBlobs) => [...prevFilmBlobs, ...data.films]);
            setThumbnailBlobNames(data.films.map((b) => b.thumbnailName));
            setTotalPages(data.totalPages);
            setHasMore(data.hasMore);
            setPageNumber((prevPage) => prevPage + 1);
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
  }, [hasMore, pageNumber]);

  useEffect(() => {
    fetchMoviesHandler();
  }, []);

  async function fetchSasToken() {
    const response = await fetch(
      `https://${process.env.REACT_APP_API_ADDRESS}/api/SAS/filmsList/`
    );
    const data = await response.json();
    return data.sasToken;
  }

  const loadMoreHandler = () => {
    setPageNumber((prevPage) => {
      return prevPage + 1;
    });
    fetchMoviesHandler();
  };

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

  if (filmBlobs.length > 0) {
    // Order by date desc
    filmBlobs.sort((a, b) => new Date(b.createdOn) - new Date(a.createdOn));
    content = (
      <Mosaic filmBlobs={filmBlobs} thumbnailBlobs={thumbnailBlobsNames} />
    );
  }

  let loadBtn = (
    <Button variant="outlined" onClick={loadMoreHandler}>
      Load more
    </Button>
  );

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
      {loadBtn}
    </div>
  );
};

export default FilmsList;
