"use client";

import { BlobServiceClient } from "@azure/storage-blob";
import { useCallback, useEffect, useState } from "react";
import styles from "./FilmsList.module.css";
import Mosaic from "./Mosaic";
import { Button, FormControl, InputAdornment, TextField } from "@mui/material";
import { Search } from "@mui/icons-material";

const FilmsList = () => {
  const [filmBlobs, setFilmBlobs] = useState([]);
  const [filteredFilmBlobs, setFilteredFilmBlobs] = useState([]);
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
            setFilmBlobs((prevFilmBlobs) => [...prevFilmBlobs, ...data.films]);
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
    console.log("process.env", process.env);
    console.log("process.env.NODE_ENV", process.env.NODE_ENV);
    console.log(
      "process.env.REACT_APP_API_ADDRESS",
      process.env.REACT_APP_API_ADDRESS
    );
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
    content = <Mosaic filmBlobs={filmBlobs} />;
  }

  let loadBtn = hasMore && (
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
