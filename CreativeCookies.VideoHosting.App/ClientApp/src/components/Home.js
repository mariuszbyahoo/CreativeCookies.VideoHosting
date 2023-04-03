import React, { Component } from "react";
import { Card } from "reactstrap";

export class Home extends Component {
  static displayName = Home.name;

  render() {
    return (
      <Card>
        <h1>Here's the homepage, a place to insert various stuff inside</h1>
      </Card>
    );
  }
}
