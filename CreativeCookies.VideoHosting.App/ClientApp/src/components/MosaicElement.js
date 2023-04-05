import { NavLink } from "react-router-dom";
import styles from "./MosaicElement.module.css";

// HACK: 1 add thumbnails SAS endpoint
// 2 insert image inside of a NavLink
// 3 TODO: find a way to retrieve and see length of a video in
// the right bottom corner
const MosaicElement = (props) => {
  return <NavLink to={"/player/" + props.film.name}>{props.film.name}</NavLink>;
};

export default MosaicElement;
