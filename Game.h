//
// Created by kellm on 2020-09-22.
//

#ifndef SFMLTEST_GAME_H
#define SFMLTEST_GAME_H

#include <SFML/Graphics.hpp>
#include <SFML/Audio.hpp>
#include <SFML/System.hpp>
#include <SFML/Window.hpp>
#include <SFML/Network.hpp>
#include "FallingSand.h"

class Game{
private:

    sf::RenderWindow* window = nullptr;
    sf::VideoMode videoMode {600, 800};
    sf::Image image{};
    sf::Sprite background{};
    sf::Texture backgroundTexture{};
    sf::Event ev{};
    FallingSand* sand;

    void init_window();
public:
    Game();
    ~Game();

    void update();
    void render();
    bool running() const;

    void update_image(const uint8_t *);

    sf::VideoMode getVideoMode() const;
};

#endif //SFMLTEST_GAME_H
