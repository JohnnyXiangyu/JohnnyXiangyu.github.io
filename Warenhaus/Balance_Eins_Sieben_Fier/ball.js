window.Ball = window.classes.Ball =
    class Ball {
        constructor(on_floor=true, position = [0, 0, 0], bound = 20.2) {
            // param: if in the initial case the ball is attached to floor, initial position of ball
            this.tilt = [0, 0]; // rad, positive = positive-axis lowering
            this.mass = 1;
            this.radius = 1;
            this.acceleration = [0, 0, 0]; // per second^2
            this.velocity = [0, 0, 0]; // per second
            this.position = Vec.of(position[0], position[1], position[2], 1); // third element should be kept zero //x,z,y
            this.gravity_acc = 9.8 * 6;
            this.prev_tilt = [0, 0];
            this.initial_drop = ((!on_floor) && (position[2] > 0));
            this.bound = bound;
            this.locked = false;
            this.score = 0;
        }

        update_tilt(dx, dy) { //[-z angle, x angle]
            this.tilt[0] = dx;
            this.tilt[1] = dy;
        }

        get_depth() {
            // return world space depth of spherical center of ball
            let result = (Mat4.inverse(this.change_basis_to(this.prev_tilt)).times(this.position))
            return result[2];
        }

        on_floor() {
            // if the ball is still able to get on to the floor
            let result = (Mat4.inverse(this.change_basis_to(this.prev_tilt)).times(this.position))
            if (result[0] > this.bound || result[1] > this.bound) {
                return false;
            }
            result = this.change_basis_to(this.tilt).times(result);
            return result[2] >= 0;
        }

        change_basis_to(tilt = [0, 0]) {
            let floor_x = (Mat4.rotation(-tilt[0], [0, 1, 0]))
                .times(Mat4.rotation(-tilt[1], [1, 0, 0]))
                .times(Vec.of(1, 0, 0, 1));
            let floor_y = (Mat4.rotation(tilt[0], [0, 1, 0]))
                .times(Mat4.rotation(-tilt[1], [1, 0, 0]))
                .times(Vec.of(0, 1, 0, 1));
            let floor_z = floor_x.cross(floor_y);
            return (Mat.of(
                [floor_x[0], floor_x[1], floor_x[2], 0],
                [floor_y[0], floor_y[1], floor_y[2], 0],
                [floor_z[0], floor_z[1], floor_z[2], 0],
                [0, 0, 0, 1]
            ).times(Mat4.translation([0, 0, 1])));
        }

        detect_collision() {
            // detect if the ball is still attached to the floor
            // if (this.on_floor) {
            //     // update acceleration
            //     this.acceleration = this.change_basis_to(this.tilt).times(Vec.of(0, 0, -this.gravity_acc));
            //     this.position[2] = 0;
            //
            //     if ((Math.abs(this.position[0]) >= 20.33) || (Math.abs(this.position[1]) >= 20.33)) {
            //         // fall out convert
            //         console.log("fall!");
            //         this.prev_tilt = this.tilt; // remember the last tilt angle
            //         this.on_floor = false;
            //     }
            //     else {
            //         this.on_floor = true;
            //         this.acceleration[2] = 0; // it's attached to the floor
            //     }
            // }
            // else {
            // calculate change of basis transformation
            this.acceleration = this.change_basis_to(this.prev_tilt).times(Vec.of(0, 0, -this.gravity_acc));
            this.acceleration[0] *= -1;

            let change_of_basis = this.change_basis_to(this.tilt);
            let change_back = Mat4.inverse(this.change_basis_to(this.prev_tilt));
            let new_coord = change_of_basis
                .times(change_back
                    .times(Vec.of(this.position[0], this.position[1], this.position[2], 1)));
            // if (new_coord[2] <= 0)
            //     new_coord[2] = 0;

            if ((new_coord[2] <= this.radius) && (Math.abs(new_coord[0]) <= this.bound) && (Math.abs(new_coord[1]) <= this.bound)
                && this.on_floor()) {
                // calculate bouncing
                if (new_coord[2] < this.radius)
                    console.log('fall');
                this.score++; // it's a callback that adds one point
                this.prev_tilt = this.tilt.slice();
                this.position = new_coord;
                this.velocity = change_of_basis
                    .times(change_back
                        .times(Vec.of(this.velocity[0], this.velocity[1], this.velocity[2], 1)));
                // this.velocity[0] = this.velocity[0] * .2;
                // this.velocity[1] = this.velocity[1] * .2;
                this.velocity[2] = Math.abs(this.velocity[2]) * 0.7;
                if (new_coord[2] <= this.radius || this.locked) {
                    this.position[2] = this.radius;
                }
                if (this.velocity[2] <= 1 || this.locked) {
                    this.velocity[2] = 0;
                    this.locked = true;
                }
            }
            //}
        }

        update(graphics_state) { // when called recalculate acceleration and velocity
            let dt = graphics_state.animation_delta_time / 1000;

            this.velocity[0] -= this.acceleration[0] * dt;
            this.velocity[1] += this.acceleration[1] * dt;
            this.velocity[2] += this.acceleration[2] * dt;
            this.position[0] += dt * this.velocity[0];
            this.position[1] += dt * this.velocity[1];
            this.position[2] += dt * this.velocity[2];

            this.detect_collision(); // update ball's status being on floor or not
        }
    }