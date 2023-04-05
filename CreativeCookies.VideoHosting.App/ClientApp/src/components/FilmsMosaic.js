import styles from "./FilmsMosaic.module.css";
import MosaicElement from "./MosaicElement";

const Mosaic = (props) => {
  return (
    <div className={styles["mosaic-wrapper"]}>
      {props.filmBlobs.map((blob, index) => (
        <MosaicElement film={blob} key={index} />
        // <NavLink to={"/player/" + blob.name} key={index}>
        //   {blob.name}
        // </NavLink>
      ))}
    </div>
  );
};

export default Mosaic;
