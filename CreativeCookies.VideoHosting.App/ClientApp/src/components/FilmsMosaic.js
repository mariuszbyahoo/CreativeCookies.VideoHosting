import { NavLink } from "react-router-dom";
import styles from "./FilmsMosaic.module.css";

const Mosaic = (props) => {
  return (
    <div className={styles["mosaic-wrapper"]}>
      {props.blobs.map((blob, index) => (
        <NavLink to={"/player/" + blob.name} key={index}>
          {blob.name}
        </NavLink>
      ))}
    </div>
  );
};

export default Mosaic;
