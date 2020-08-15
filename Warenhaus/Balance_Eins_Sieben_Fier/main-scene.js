window.BalanceBall = window.classes.BalanceBall =
    class BalanceBall extends Scene_Component {
        constructor(context, control_box)     // The scene begins by requesting the camera, shapes, and materials it will need.
        {
            super(context, control_box);    // First, include a secondary Scene that provides movement controls:
            if (!context.globals.has_controls)
                context.register_scene_component(new Movement_Controls(context, control_box.parentElement.insertCell()));

            context.globals.graphics_state.camera_transform = Mat4.look_at(Vec.of(45, 40, 0), Vec.of(0, 0, 0), Vec.of(0, 1, 0));

            const r = context.width / context.height;
            context.globals.graphics_state.projection_transform = Mat4.perspective(Math.PI / 4, r, .1, 1000);

            const shapes = {
                ball: new Subdivision_Sphere(4),
                square: new Square()
            };

            this.submit_shapes(context, shapes);

            this.materials =
                {
                    phong: context.get_instance(Phong_Shader).material(Color.of(1, 1, 0, 1)),
                    stars: context.get_instance(Phong_Shader).material(
                        Color.of(0,0,0,1),
                        {
                            ambient: 1.0,
                            texture: context.get_instance("assets/stars.png", false)
                        }
                    ),
                    ice: context.get_instance(Fake_Bump_Map).material(
                        Color.of(0.15,0.15,0.15,1),
                        {
                            ambient: 1.0,
                            diffusivity: 0.5,
                            texture: context.get_instance("assets/frozenice.png", false)
                        }
                    ),
                    ucla: context.get_instance(Fake_Bump_Map).material(
                        Color.of(0.15,0.15,0.15,1),
                        {
                            ambient: 1.0,
                            diffusivity: 0.5,
                            texture: context.get_instance("assets/ucla.jpg", false)
                        }
                    ),
                };

            // position is [x,z,y]
            this.ball_state = new Ball(false, [0,0,30]);

            this.lights = [new Light(Vec.of(5, 5, 5, 1), Color.of(0, 1, 1, 1), 100000)];

            this.ball_transform = Mat4.identity().times(Mat4.translation([0,0,0]));
            this.rotating = false;

            // rotation control
            this.isPressed = false;
            this.z_rotate = (Math.random(2) - 1 )* 0.1;
            this.x_rotate = (Math.random(2) - 1 )* 0.1;
            this.MAX_ROTATE = Math.PI * .2;

            this.floor_transform = Mat4.identity();
            //timer
            this.startTime = 0;
            this.endTime = 0;
            this.triggered = 0;
            this.timer = 0;
        }

        start()
        {
            this.startTime = new Date();
        }

        end(dt=0)
        {
            this.timer += dt;
        }

        rotate_control(isX_axis, isZ_axis, ispressed, orientation)
        {

            this.triggered += 1;
            this.isPressed = ispressed;
            // const t = graphics_state.animation_time / 1000, dt = graphics_state.animation_delta_time / 1000;
            if (isX_axis)
            {
                if (ispressed)
                {
                    if (orientation && this.x_rotate < 1)
                        this.x_rotate += .02;
                    else if (!orientation && this.x_rotate > -1)
                        this.x_rotate -= .02;
                }
                else
                {
                    if (this.x_rotate > -0.02 && this.x_rotate < 0.02)
                        this.x_rotate = 0;
                    if (this.x_rotate > 0)
                        this.x_rotate -= .02;
                    else if (this.x_rotate < 0)
                        this.x_rotate += .02;
                }
            }
            else if (isZ_axis)
            {
                if (ispressed)
                {
                    if (orientation && this.z_rotate < 1)
                        this.z_rotate += .02;
                    else if (!orientation && this.z_rotate > -1)
                        this.z_rotate -= .02;
                }
                else
                {
                    if (this.z_rotate > -0.02 && this.z_rotate < 0.02)
                        this.z_rotate = 0;
                    if (this.z_rotate > 0)
                        this.z_rotate -= .02;
                    else if (this.z_rotate < 0)
                        this.z_rotate += .02;
                }
            }
            else{}
        }

        reset() { // reset the board and the ball
            this.z_rotate = (Math.random(2) - 1 )* 0.1;
            this.x_rotate = (Math.random(2) - 1 )* 0.1;
            // this.z_rotate = 0;
            // this.x_rotate = 0;
            this.ball_state = new Ball(false, [0,0,30]);

            this.timer = 0;
            this.triggered = 0;
        }

        make_control_panel() {
            this.key_triggered_button( "Tilt Up",   [ "w" ],
                () => this.rotate_control(false, true, true, true), undefined,
                () => this.isPressed = false);

            this.key_triggered_button("Tilt Right", ["d"],
                () => this.rotate_control(true, false, true, false), undefined,
                () => this.isPressed = false);

            this.key_triggered_button("Tilt Down", ["s"],
                () => this.rotate_control(false, true, true, false), undefined,
                () => this.isPressed = false);

            this.key_triggered_button("Tilt Left", ["a"],
                () => this.rotate_control(true, false, true, true), undefined,
                () => this.isPressed = false);

            this.key_triggered_button("Reset", ["x"], () => this.reset());
        }

        draw_ball(graphics_state) {
            const t = graphics_state.animation_time / 1000, dt = graphics_state.animation_delta_time / 1000;
            this.x_angle = this.MAX_ROTATE * this.x_rotate;
            this.z_angle = this.MAX_ROTATE * this.z_rotate;

            this.still = this.x_angle === 0 && this.z_angle === 0;

            // velocity needs Vec.of(0,0,0);
            // ball_pos needs Vec.of(x,0,z);

            this.velocity = Vec.of(0,0,0);
            this.velocity[0] = this.ball_state.velocity[0];
            this.velocity[1] = this.ball_state.velocity[2];
            this.velocity[2] = this.ball_state.velocity[1];

            this.speed = Vec.of(this.velocity[0],0,this.velocity[2]).norm();

            if (this.velocity !== Vec.of(0, 0, 0)) {
                this.rotating = true;
                this.rolling_axis = this.velocity.cross([0, -1, 0]);
            } else {
                this.rotating = false;
                this.rolling_axis = [0, 0, 1];
            }

            let angles = this.ball_state.prev_tilt;

            this.ball_pos = Vec.of(0,0,0);
            this.ball_pos[0] = this.ball_state.position[0];
            this.ball_pos[1] = this.ball_state.position[2];
            this.ball_pos[2] = this.ball_state.position[1];

            this.ball_transform = Mat4.identity()
                .times(Mat4.rotation(angles[1], [1, 0, 0]))
                .times(Mat4.rotation(angles[0], [0, 0, 1]))
                .times(Mat4.translation(this.ball_pos));

            // if(!this.rotating) {
                this.shapes.ball.draw(graphics_state, this.ball_transform, this.materials.stars);
            // }

            this.rotation_angle = this.speed * Math.PI / 4;

            if(this.rotating) {
                this.ball_transform = this.ball_transform.times(Mat4.rotation(this.rotation_angle, this.rolling_axis));
                this.shapes.ball.draw(graphics_state, this.ball_transform, this.materials.stars);
            }
        }

        display(graphics_state){
            graphics_state.lights = this.lights;        // Use the lights stored in this.lights.
            const t = graphics_state.animation_time / 1000, dt = graphics_state.animation_delta_time / 1000;

            //timer
            if(this.ball_state.on_floor())
                this.timer += 10 * dt;
            document.getElementById('scoreboard').innerHTML = "Score Board: "+Math.round(this.timer).toString();

            this.ball_state.update_tilt(this.MAX_ROTATE * this.z_rotate, this.MAX_ROTATE * this.x_rotate);
            this.ball_state.update(graphics_state);

            // draw floor
            this.floor_transform = Mat4.identity()
                .times(Mat4.rotation(this.MAX_ROTATE * this.x_rotate, Vec.of(1, 0, 0)))
                .times(Mat4.rotation(this.MAX_ROTATE * this.z_rotate, Vec.of(0, 0, 1)));
            this.shapes.square.draw(graphics_state, this.floor_transform, this.materials.ice);

            // draw ball
            this.draw_ball(graphics_state);

            if ((this.ball_state.get_depth() <= -100))
            {
                setTimeout(this.reset(), 2000);
            }
        }
    };